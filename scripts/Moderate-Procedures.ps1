<#
.SYNOPSIS
Reviews Procedure Exchange reports and reversibly hides published text.
.DESCRIPTION
Owner-invoked administration only. No visitor action invokes this script.
Requires CLOUDFLARE_API_TOKEN in the current process. It changes no permissions,
deletes no content and does not automatically turn reports into moderation.
#>
param(
    [ValidateSet('List','Hide','Unhide','Resolve')][string]$Action = 'List',
    [string]$PlaybookId
)
$ErrorActionPreference = 'Stop'
if (-not $env:CLOUDFLARE_API_TOKEN) { throw 'Set CLOUDFLARE_API_TOKEN in this process before using owner administration.' }
if ($PlaybookId -and $PlaybookId -notmatch '^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$') { throw 'Supply a valid procedure identifier.' }
if ($Action -ne 'List' -and -not $PlaybookId) { throw 'A single PlaybookId is required for this action.' }
$projectRoot = Split-Path $PSScriptRoot -Parent
$hosting = Get-Content -LiteralPath (Join-Path $projectRoot 'docs/hosting.json') -Raw | ConvertFrom-Json
$databaseId = $hosting.databaseId
if (-not $hosting.accountId -or -not $databaseId) { throw 'The existing hosting account and D1 database identifiers are required.' }
$timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
$parameters = @()
switch ($Action) {
    'List' {
        $sql = 'SELECT r.playbook_id AS playbookId,p.title,r.reason,r.note,r.created_at AS reportedAt,p.withdrawn_at AS withdrawnAt,p.hidden_at AS hiddenAt FROM procedure_reports r JOIN procedure_playbooks p ON p.id=r.playbook_id WHERE r.resolved_at IS NULL'
        if ($PlaybookId) { $sql += ' AND r.playbook_id=?'; $parameters = @($PlaybookId) }
        $sql += ' ORDER BY r.created_at LIMIT 50'
    }
    'Hide' { $sql = 'UPDATE procedure_playbooks SET hidden_at=COALESCE(hidden_at,?) WHERE id=?'; $parameters = @($timestamp,$PlaybookId) }
    'Unhide' { $sql = 'UPDATE procedure_playbooks SET hidden_at=NULL WHERE id=?'; $parameters = @($PlaybookId) }
    'Resolve' { $sql = 'UPDATE procedure_reports SET resolved_at=? WHERE playbook_id=? AND resolved_at IS NULL'; $parameters = @($timestamp,$PlaybookId) }
}
$requestBody = @{sql=$sql;params=$parameters} | ConvertTo-Json -Depth 4 -Compress
$endpoint = "https://api.cloudflare.com/client/v4/accounts/$($hosting.accountId)/d1/database/$databaseId/query"
$response = Invoke-RestMethod -Method Post -Uri $endpoint -Headers @{Authorization=('Bearer '+$env:CLOUDFLARE_API_TOKEN)} -ContentType 'application/json' -Body $requestBody
if (-not $response.success -or @($response.result | Where-Object { -not $_.success }).Count) { throw 'D1 did not confirm the requested moderation action.' }
if ($Action -eq 'List') { $response.result.results | Select-Object playbookId,title,reason,note,reportedAt,withdrawnAt,hiddenAt }
else { Write-Host "Action $Action completed; affected rows: $($response.result[0].meta.changes)." }
