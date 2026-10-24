<#
.SYNOPSIS
Configures BinSite membership billing and Cloudflare Pages production secrets.
.DESCRIPTION
Run manually as the site owner after the commerce schema and functions are deployed,
and again when changing the provider, price, API key, webhook, or billing portal.
No scheduler invokes this script. It creates or reuses a Stripe monthly product/price,
sets up the hosted billing portal, registers signed webhooks, and writes configuration
to the existing Pages production environment. Lemon Squeezy needs an approved store
and a monthly subscription variant; its public API does not create products.
Secrets come from process environment variables or masked prompts, are held in memory,
and are never written to the repository, a local file, or standard output.
.EXAMPLE
pwsh -File C:\E\Claude\WebSiteleri\BinSite\scripts\Configure-Revenue.ps1 -Provider Lemon -Mode Test
#>
[CmdletBinding()]
param(
    [ValidateSet('Stripe', 'Lemon', 'Both')][string]$Provider = 'Lemon',
    [ValidateSet('Test', 'Live')][string]$Mode = 'Test',
    [string]$PagesProject,
    [uri]$SiteUrl,
    [string]$CloudflareAccountId,
    [ValidateRange(100, 100000)][int]$MonthlyCents = 500,
    [string]$LemonStoreId = $env:LEMON_STORE_ID,
    [string]$LemonVariantId = $env:LEMON_VARIANT_ID,
    [switch]$NoPrompt,
    [switch]$FinalizeWebhooks
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$projectRoot = Split-Path $PSScriptRoot -Parent
$hostingPath = Join-Path $projectRoot 'docs/hosting.json'
$hosting = if (Test-Path -LiteralPath $hostingPath) { Get-Content -LiteralPath $hostingPath -Raw | ConvertFrom-Json -AsHashtable } else { @{} }
if (-not $PagesProject -and $hosting.ContainsKey('projectName')) { $PagesProject = [string]$hosting.projectName }
if (-not $CloudflareAccountId -and $hosting.ContainsKey('accountId')) { $CloudflareAccountId = [string]$hosting.accountId }
if (-not $SiteUrl -and $hosting.ContainsKey('siteUrl')) { $SiteUrl = [uri]$hosting.siteUrl }
if (-not $SiteUrl -and $env:BINSITE_SITE_URL) { $SiteUrl = [uri]$env:BINSITE_SITE_URL }
if ($PagesProject -notmatch '^[a-z0-9][a-z0-9-]{0,61}[a-z0-9]$' -or $CloudflareAccountId -notmatch '^[a-f0-9]{32}$' -or -not $SiteUrl) { throw 'Set projectName, accountId, and the verified siteUrl in docs/hosting.json, or supply the corresponding override parameters.' }
if ($SiteUrl.Scheme -ne 'https' -or $SiteUrl.AbsolutePath -ne '/' -or $SiteUrl.Query -or $SiteUrl.Fragment -or $SiteUrl.UserInfo) { throw 'SiteUrl must be an HTTPS origin without a path, query, fragment, or credentials.' }
$siteOrigin = $SiteUrl.GetLeftPart([System.UriPartial]::Authority)
$stripeVersion = '2025-02-24.acacia'
$settings = @{}
$oldStripeHooks = @()
$oldLemonHooks = @()
$stripeKey = $null
$lemonKey = $null
$cloudflareToken = $null

function Get-SecretValue([string]$Name) {
    $value = [Environment]::GetEnvironmentVariable($Name, 'Process')
    if ($value) { return $value }
    if ($NoPrompt) { throw "Set the process environment variable $Name before invoking this script." }
    $secure = Read-Host "$Name (hidden)" -AsSecureString
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { $value = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer); $secure.Dispose() }
    if (-not $value) { throw "$Name was not supplied." }
    return $value
}

function New-RandomSecret {
    return [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(20)).ToLowerInvariant()
}

function Invoke-Stripe([string]$Path, [hashtable]$Form = $null, [string]$Method = 'GET', [string]$Idempotency = '') {
    $headers = @{ Authorization = "Bearer $stripeKey"; 'Stripe-Version' = $stripeVersion }
    if ($Idempotency) { $headers['Idempotency-Key'] = $Idempotency }
    $arguments = @{ Uri = "https://api.stripe.com/v1/$Path"; Method = $Method; Headers = $headers; TimeoutSec = 30 }
    if ($null -ne $Form) {
        $arguments.ContentType = 'application/x-www-form-urlencoded'
        $arguments.Body = ($Form.GetEnumerator() | ForEach-Object { [uri]::EscapeDataString([string]$_.Key) + '=' + [uri]::EscapeDataString([string]$_.Value) }) -join '&'
    }
    try { return Invoke-RestMethod @arguments }
    catch { throw "Stripe operation failed: $Method $($Path.Split('?')[0]). No secrets were printed." }
}

function Invoke-Lemon([string]$Path, [object]$Body = $null, [string]$Method = 'GET') {
    $arguments = @{ Uri = "https://api.lemonsqueezy.com/v1/$Path"; Method = $Method; Headers = @{ Authorization = "Bearer $lemonKey"; Accept = 'application/vnd.api+json' }; TimeoutSec = 30 }
    if ($null -ne $Body) { $arguments.ContentType = 'application/vnd.api+json'; $arguments.Body = $Body | ConvertTo-Json -Depth 30 -Compress }
    try { return Invoke-RestMethod @arguments }
    catch { throw "Lemon Squeezy operation failed: $Method $($Path.Split('?')[0]). No secrets were printed." }
}

function Invoke-Cloudflare([string]$Path, [object]$Body = $null, [string]$Method = 'GET') {
    $arguments = @{ Uri = "https://api.cloudflare.com/client/v4/accounts/$CloudflareAccountId/pages/projects/$PagesProject$Path"; Method = $Method; Headers = @{ Authorization = "Bearer $cloudflareToken" }; TimeoutSec = 30 }
    if ($null -ne $Body) { $arguments.ContentType = 'application/json'; $arguments.Body = $Body | ConvertTo-Json -Depth 60 -Compress }
    try { $response = Invoke-RestMethod @arguments | ConvertTo-Json -Depth 80 | ConvertFrom-Json -AsHashtable; if (-not $response.success) { throw 'Provider rejected request.' }; return $response.result }
    catch { throw 'Cloudflare Pages configuration operation failed. Verify the account, project, and token permissions. No secrets were printed.' }
}

function Set-Setting([string]$Name, [string]$Value, [bool]$Secret = $false) {
    $settings[$Name] = @{ type = $(if ($Secret) { 'secret_text' } else { 'plain_text' }); value = $Value }
}

try {
    $cloudflareToken = Get-SecretValue 'CLOUDFLARE_API_TOKEN'
    $project = Invoke-Cloudflare ''
    if (-not $project.ContainsKey('deployment_configs') -or -not $project.deployment_configs.ContainsKey('production')) { throw 'The Pages project does not have a production deployment configuration.' }
    $production = $project.deployment_configs.production
    if (-not $production.ContainsKey('d1_databases') -or -not $production.d1_databases.ContainsKey('DB')) { throw 'Bind the commerce D1 database as DB before enabling billing.' }
    if ($FinalizeWebhooks) {
        $status = Invoke-RestMethod -Uri "$siteOrigin/api/commerce/status" -TimeoutSec 20 | ConvertTo-Json -Depth 30 | ConvertFrom-Json -AsHashtable
        $expectedVersion = $production.env_vars.COMMERCE_CONFIG_VERSION.value
        if (-not $expectedVersion -or $status.configurationVersion -ne $expectedVersion) { throw 'Deploy the new configuration before retiring old webhook endpoints. Existing endpoints were preserved.' }
        if ($production.env_vars.ContainsKey('STRIPE_WEBHOOK_ID')) {
            $stripeKey = Get-SecretValue 'STRIPE_SECRET_KEY'
            $hooks = Invoke-Stripe 'webhook_endpoints?limit=100'
            foreach ($hook in @($hooks.data | Where-Object { $_.url -eq "$siteOrigin/api/commerce/webhooks/stripe" -and $_.id -ne $production.env_vars.STRIPE_WEBHOOK_ID.value -and $_.status -eq 'enabled' })) {
                $null = Invoke-Stripe "webhook_endpoints/$($hook.id)" @{ disabled = 'true' } 'POST'
            }
        }
        if ($production.env_vars.ContainsKey('LEMON_WEBHOOK_ID')) {
            $lemonKey = Get-SecretValue 'LEMON_API_KEY'
            $hooks = Invoke-Lemon 'webhooks?page[size]=100'
            foreach ($hook in @($hooks.data | Where-Object { $_.attributes.url -eq "$siteOrigin/api/commerce/webhooks/lemon" -and [string]$_.id -ne $production.env_vars.LEMON_WEBHOOK_ID.value })) {
                $null = Invoke-Lemon "webhooks/$($hook.id)" $null 'DELETE'
            }
        }
        Write-Host 'Active production configuration verified. Earlier webhook endpoints retired.'
        return
    }
    Set-Setting 'COMMERCE_CONFIG_VERSION' ([guid]::NewGuid().ToString())
    Set-Setting 'CREATOR_MONTHLY_CENTS' ([string]$MonthlyCents)
    if (-not $production.ContainsKey('env_vars') -or -not $production.env_vars.ContainsKey('COMMERCE_RATE_SALT')) { Set-Setting 'COMMERCE_RATE_SALT' (New-RandomSecret) $true }

    if ($Provider -in @('Stripe', 'Both')) {
        $stripeKey = Get-SecretValue 'STRIPE_SECRET_KEY'
        $expectedPrefix = if ($Mode -eq 'Live') { 'sk_live_' } else { 'sk_test_' }
        if (-not $stripeKey.StartsWith($expectedPrefix, [StringComparison]::Ordinal)) { throw "Stripe key does not match selected mode $Mode." }
        Write-Host "Configuring Stripe $Mode monthly membership at $MonthlyCents USD cents."
        $lookup = "binsite_creator_monthly_usd_$MonthlyCents"
        $prices = Invoke-Stripe "prices?lookup_keys%5B0%5D=$lookup&active=true&limit=10"
        $price = @($prices.data | Where-Object { $_.currency -eq 'usd' -and $_.unit_amount -eq $MonthlyCents -and $_.recurring.interval -eq 'month' -and $_.recurring.interval_count -eq 1 }) | Select-Object -First 1
        if (-not $price) {
            $product = Invoke-Stripe 'products' @{ name = 'BinSite Creator'; description = 'Private cloud project storage: 100 JSON projects, up to 64 KB each.'; 'metadata[binsite_plan]' = 'creator-monthly' } 'POST' 'binsite-creator-product-v1'
            $price = Invoke-Stripe 'prices' @{ product = $product.id; currency = 'usd'; unit_amount = [string]$MonthlyCents; 'recurring[interval]' = 'month'; lookup_key = $lookup; 'metadata[binsite_plan]' = 'creator-monthly' } 'POST' "binsite-creator-price-$MonthlyCents-v1"
        }
        $portalList = Invoke-Stripe 'billing_portal/configurations?active=true&limit=100'
        $portal = @($portalList.data | Where-Object { $_.business_profile.headline -eq 'BinSite Creator Membership' }) | Select-Object -First 1
        $portalForm = @{ 'business_profile[headline]' = 'BinSite Creator Membership'; 'features[subscription_cancel][enabled]' = 'true'; 'features[subscription_cancel][mode]' = 'at_period_end'; 'features[payment_method_update][enabled]' = 'true'; 'features[invoice_history][enabled]' = 'true' }
        if ($portal) { $portal = Invoke-Stripe "billing_portal/configurations/$($portal.id)" $portalForm 'POST' }
        else { $portal = Invoke-Stripe 'billing_portal/configurations' $portalForm 'POST' 'binsite-creator-portal-v1' }
        $hookUrl = "$siteOrigin/api/commerce/webhooks/stripe"
        $hooks = Invoke-Stripe 'webhook_endpoints?limit=100'
        $oldStripeHooks = @($hooks.data | Where-Object { $_.url -eq $hookUrl -and $_.status -eq 'enabled' })
        $hookForm = @{ url = $hookUrl; api_version = $stripeVersion; description = 'BinSite membership entitlement sync' }
        $events = @('checkout.session.completed', 'checkout.session.async_payment_succeeded', 'customer.subscription.created', 'customer.subscription.updated', 'customer.subscription.deleted', 'invoice.paid', 'invoice.payment_failed', 'charge.refunded', 'charge.dispute.created')
        for ($i = 0; $i -lt $events.Count; $i++) { $hookForm["enabled_events[$i]"] = $events[$i] }
        $hook = Invoke-Stripe 'webhook_endpoints' $hookForm 'POST' ([guid]::NewGuid().ToString())
        Set-Setting 'STRIPE_SECRET_KEY' $stripeKey $true
        Set-Setting 'STRIPE_PRICE_ID' $price.id
        Set-Setting 'STRIPE_PORTAL_CONFIGURATION_ID' $portal.id
        Set-Setting 'STRIPE_WEBHOOK_SECRET' $hook.secret $true
        Set-Setting 'STRIPE_WEBHOOK_ID' $hook.id
    }

    if ($Provider -in @('Lemon', 'Both')) {
        $lemonKey = Get-SecretValue 'LEMON_API_KEY'
        $stores = Invoke-Lemon 'stores?page[size]=100'
        if (-not $LemonStoreId) {
            if (@($stores.data).Count -ne 1) { throw 'Supply -LemonStoreId to select the intended store; multiple or no stores were returned.' }
            $LemonStoreId = [string]$stores.data[0].id
        }
        if ($LemonStoreId -notmatch '^\d+$' -or -not @($stores.data | Where-Object { [string]$_.id -eq $LemonStoreId }).Count) { throw 'The selected Lemon Squeezy store was not returned for this API key.' }
        if (-not $LemonVariantId) {
            $variants = Invoke-Lemon 'variants?page[size]=100'
            $candidates = @($variants.data | Where-Object { $_.attributes.name -like '*BinSite*' -and $_.attributes.is_subscription -and $_.attributes.interval -eq 'month' -and $_.attributes.interval_count -eq 1 -and $_.attributes.price -eq $MonthlyCents })
            if ($candidates.Count -ne 1) { throw 'Create/select the BinSite monthly subscription product in the approved Lemon Squeezy store, then supply -LemonVariantId. The public API does not create subscription products.' }
            $LemonVariantId = [string]$candidates[0].id
        }
        if ($LemonVariantId -notmatch '^\d+$') { throw 'LemonVariantId must contain digits.' }
        $variant = Invoke-Lemon "variants/$LemonVariantId"
        $product = Invoke-Lemon "products/$($variant.data.attributes.product_id)"
        if ([string]$product.data.attributes.store_id -ne $LemonStoreId -or -not $variant.data.attributes.is_subscription -or $variant.data.attributes.interval -ne 'month' -or $variant.data.attributes.interval_count -ne 1 -or $variant.data.attributes.price -ne $MonthlyCents) { throw 'The selected Lemon Squeezy variant must belong to this store and match the configured monthly amount.' }
        $store = @($stores.data | Where-Object { [string]$_.id -eq $LemonStoreId })[0]
        if ($store.attributes.currency -ne 'USD') { throw 'The selected Lemon Squeezy store must use USD to match the displayed plan price.' }
        $hookUrl = "$siteOrigin/api/commerce/webhooks/lemon"
        $hooks = Invoke-Lemon 'webhooks?page[size]=100'
        $oldLemonHooks = @($hooks.data | Where-Object { $_.attributes.url -eq $hookUrl -and [string]$_.attributes.store_id -eq $LemonStoreId })
        $secret = if ($env:LEMON_WEBHOOK_SECRET) { $env:LEMON_WEBHOOK_SECRET } else { New-RandomSecret }
        $events = @('subscription_created', 'subscription_updated', 'subscription_cancelled', 'subscription_resumed', 'subscription_expired', 'subscription_paused', 'subscription_unpaused', 'subscription_payment_success', 'subscription_payment_failed', 'subscription_payment_recovered', 'subscription_payment_refunded', 'order_refunded')
        $attributes = @{ url = $hookUrl; events = $events; secret = $secret; test_mode = ($Mode -eq 'Test') }
        $lemonHook = Invoke-Lemon 'webhooks' @{ data = @{ type = 'webhooks'; attributes = $attributes; relationships = @{ store = @{ data = @{ type = 'stores'; id = $LemonStoreId } } } } } 'POST'
        Set-Setting 'LEMON_API_KEY' $lemonKey $true
        Set-Setting 'LEMON_STORE_ID' $LemonStoreId
        Set-Setting 'LEMON_VARIANT_ID' $LemonVariantId
        Set-Setting 'LEMON_TEST_MODE' ($Mode -eq 'Test').ToString().ToLowerInvariant()
        Set-Setting 'LEMON_WEBHOOK_SECRET' $secret $true
        Set-Setting 'LEMON_WEBHOOK_ID' $lemonHook.data.id
        Write-Host "Configured Lemon Squeezy $Mode store and monthly subscription variant."
    }

    $environment = @{}
    # PATCH merges only changed keys; GET returns redacted secret values that must not be written back.
    foreach ($name in $settings.Keys) { $environment[$name] = $settings[$name] }
    $null = Invoke-Cloudflare '' @{ deployment_configs = @{ production = @{ env_vars = $environment } } } 'PATCH'
    $verification = Invoke-Cloudflare ''
    foreach ($name in $settings.Keys) { if (-not $verification.deployment_configs.production.env_vars.ContainsKey($name)) { throw "Cloudflare did not confirm binding $name." } }
    Write-Host 'Production settings saved. Values were not printed or written to disk.'
    Write-Host 'Deploy the current Pages source again so the new environment is active, then perform a provider test checkout and verify cancellation/refund before switching to live keys.'
    Write-Host 'Run this script again with -FinalizeWebhooks after deployment to retire earlier endpoints; it verifies the active configuration version first.'
    Write-Host "Membership page: $siteOrigin/uyelik/"
}
finally {
    $stripeKey = $null
    $lemonKey = $null
    $cloudflareToken = $null
    $settings.Clear()
}
