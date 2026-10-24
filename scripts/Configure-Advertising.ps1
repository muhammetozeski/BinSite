param(
    [Parameter(Mandatory)][ValidatePattern('^ca-pub-\d{16}$')][string]$PublisherId,
    [Parameter(Mandatory)][ValidatePattern('^\d{5,20}$')][string]$SlotId,
    [switch]$ConsentPlatformReady
)
$ErrorActionPreference = 'Stop'
if (-not $env:CLOUDFLARE_API_TOKEN) { throw 'Set CLOUDFLARE_API_TOKEN in this process before configuring hosting.' }
$projectRoot = Split-Path $PSScriptRoot -Parent
$hosting = Get-Content -LiteralPath (Join-Path $projectRoot 'docs/hosting.json') -Raw | ConvertFrom-Json
$headers = @{ Authorization = "Bearer $env:CLOUDFLARE_API_TOKEN" }
$url = "https://api.cloudflare.com/client/v4/accounts/$($hosting.accountId)/pages/projects/$($hosting.projectName)"
$project = Invoke-RestMethod -Uri $url -Headers $headers
if (-not $project.success) { throw 'Could not read the Pages project.' }
$variables = @{}
# PATCH merges these keys. GET redacts existing secret values, so never copy them back.
$variables.ADSENSE_CLIENT = @{ type = 'plain_text'; value = $PublisherId }
$variables.ADSENSE_SLOT = @{ type = 'plain_text'; value = $SlotId }
$variables.ADSENSE_CONSENT_READY = @{ type = 'plain_text'; value = $ConsentPlatformReady.IsPresent.ToString().ToLowerInvariant() }
$payload = @{ deployment_configs = @{ production = @{ env_vars = $variables } } } | ConvertTo-Json -Depth 15
$result = Invoke-RestMethod -Method Patch -Uri $url -Headers $headers -ContentType 'application/json' -Body $payload
if (-not $result.success) { throw 'Advertising configuration could not be saved.' }
Write-Host 'Publisher settings saved. Run the existing GitHub publication workflow to apply them.'
Write-Host 'Use ConsentPlatformReady only after the approved publisher account and its required Google Privacy & messaging or certified consent platform have been configured.'
