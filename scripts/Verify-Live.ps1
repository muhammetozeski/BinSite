<#
.SYNOPSIS
Verifies new published product paths and maintains an evidence ledger.
.DESCRIPTION
The owner or publishing agent runs this after a successful Pages deployment.
It is not scheduled. Unchanged previously verified paths are skipped unless
-Recheck is supplied. -UpdateRegistry writes the owner's SiteYonetimi site list.
This checks HTTP delivery, not browser rendering, audio, or user interaction.
#>
param([switch]$Recheck, [switch]$UpdateRegistry)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$hosting = Get-Content -LiteralPath (Join-Path $projectRoot 'docs/hosting.json') -Raw | ConvertFrom-Json
$siteOrigin = $hosting.siteUrl.TrimEnd('/')
$ledgerPath = Join-Path $projectRoot 'publication.json'
$verified = @()
$verifiedUrls = @{}
if (Test-Path -LiteralPath $ledgerPath) {
    $ledger = Get-Content -LiteralPath $ledgerPath -Raw | ConvertFrom-Json
    $verified = @($ledger.verifiedPaths)
    foreach ($slug in $verified) { $verifiedUrls[$slug] = "$siteOrigin/$slug/" }
    if ($ledger.verifiedUrls) {
        foreach ($property in $ledger.verifiedUrls.PSObject.Properties) { $verifiedUrls[$property.Name] = $property.Value }
    }
}
$catalog = Invoke-RestMethod -Uri "$siteOrigin/catalog.json"
if ($catalog -isnot [System.Array]) { $catalog = @($catalog) }
$newChecks = 0
foreach ($product in $catalog) {
    if ($product.slug -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') { throw 'Unexpected product path in the public catalog.' }
    $productUrl = "$siteOrigin/$($product.slug)/"
    if ($product.PSObject.Properties.Name -contains 'url') {
        $externalUri = $null
        if (-not [uri]::TryCreate([string]$product.url, [UriKind]::Absolute, [ref]$externalUri) -or $externalUri.Scheme -ne 'https' -or $externalUri.UserInfo -or $externalUri.Query -or $externalUri.Fragment) { throw "Invalid external product URL: $($product.slug)" }
        $productUrl = $externalUri.AbsoluteUri
    }
    if (-not $Recheck -and $product.slug -in $verified -and $verifiedUrls[$product.slug] -ceq $productUrl) { continue }
    $response = Invoke-WebRequest -Uri $productUrl
    if ($response.StatusCode -ne 200 -or $response.Content -notmatch '<html\b') { throw "Product delivery failed: $($product.slug)" }
    if ($response.Content -notmatch '<html\b[^>]*lang\s*=\s*["'']en(?:-[a-z]+)?["'']') { throw "Published product is not marked as English: $($product.slug)" }
    $themeAttribute = 'data-bs-theme'
    $assetAttribute = 'data-binsite-appearance'
    if ($product.appearance) {
        $themeAttribute = [string]$product.appearance.themeAttribute
        $assetAttribute = [string]$product.appearance.assetAttribute
        if ($themeAttribute -cnotmatch '^data-[a-z][a-z0-9-]*$' -or $assetAttribute -cnotmatch '^data-[a-z][a-z0-9-]*$') { throw "Invalid appearance evidence attributes: $($product.slug)" }
    }
    $themePattern = '<html\b[^>]*' + [regex]::Escape($themeAttribute) + '\s*=\s*["'']dark["'']'
    $scriptPattern = '<script\b[^>]*' + [regex]::Escape($assetAttribute) + '\s*=\s*["'']script["'']'
    $stylePattern = '<link\b[^>]*' + [regex]::Escape($assetAttribute) + '\s*=\s*["'']style["'']'
    if ($response.Content -notmatch $themePattern) { throw "Published product is missing its dark default: $($product.slug)" }
    if ($response.Content -notmatch $scriptPattern -or $response.Content -notmatch $stylePattern) { throw "Published appearance dependencies are missing: $($product.slug)" }
    $verified += $product.slug
    $verifiedUrls[$product.slug] = $productUrl
    $newChecks++
}
$activeSlugs = @($catalog.slug)
$verified = @($verified | Sort-Object -Unique | Where-Object { $_ -in $activeSlugs })
$activeUrls = @{}
foreach ($slug in $verified) { $activeUrls[$slug] = $verifiedUrls[$slug] }
$localTime = [System.TimeZoneInfo]::ConvertTimeBySystemTimeZoneId([datetime]::UtcNow, 'Turkey Standard Time').ToString('yyyy-MM-dd HH:mm:ss')
@{origin=$siteOrigin;verifiedAtIstanbul=$localTime;liveCatalogCount=$catalog.Count;verifiedPathCount=$verified.Count;verifiedPaths=$verified;verifiedUrls=$activeUrls;scope='HTTP delivery, English document language, dark default and appearance dependencies; interaction and API checks are recorded separately.'} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $ledgerPath -Encoding utf8
if ($UpdateRegistry) {
    $registryPath = Join-Path $env:USERPROFILE '.agents/skills/SiteYonetimi/references/siteler.md'
    if (-not (Test-Path -LiteralPath $registryPath)) { throw 'SiteYonetimi site registry was not found.' }
    $registry = [System.IO.File]::ReadAllText($registryPath)
    $registry = $registry -replace 'Son güncelleme: \d{4}-\d{2}-\d{2}', ('Son güncelleme: ' + $localTime.Substring(0,10))
    $lines = @($registry -split '\r?\n' | Where-Object { $_ -notmatch '^\| BinSite \|' -and $_ -notmatch '^\| .* \(BinSite\) \|' })
    $rows = [System.Collections.Generic.List[string]]::new()
    $rootLink = $projectRoot.Replace('\','/')
    $rows.Add("| BinSite | Yayında | [BinSite]($siteOrigin/) | [$projectRoot]($rootLink) | [$($hosting.repo)](https://github.com/$($hosting.repo)) | ``$($hosting.projectName)`` |")
    foreach ($product in $catalog) {
        # Independently hosted products maintain their own source/hosting registry row.
        if ($product.url) { continue }
        $name = $product.name.Replace('|','-')
        $localPath = Join-Path $projectRoot "public/$($product.slug)"
        $localLink = $localPath.Replace('\','/')
        $rows.Add("| $name (BinSite) | Yayında | [$name]($siteOrigin/$($product.slug)/) | [$localPath]($localLink) | [$($hosting.repo)](https://github.com/$($hosting.repo)) | ``$($hosting.projectName)`` |")
    }
    [System.IO.File]::WriteAllText($registryPath, ($lines -join "`n").TrimEnd() + "`n" + ($rows -join "`n") + "`n", [System.Text.UTF8Encoding]::new($false))
}
Write-Host "Public catalog: $($catalog.Count); verified product paths: $($verified.Count); newly checked: $newChecks."
