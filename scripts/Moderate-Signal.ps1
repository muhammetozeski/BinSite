<#
.SYNOPSIS
Lists Signal Foundry reports, reversibly hides challenges, or prunes expired play sessions.
.DESCRIPTION
Owner-invoked only; requires CLOUDFLARE_API_TOKEN in this process.
Pruning is bounded and affects only already expired attempts and replay entries.
#>
param(
    [ValidateSet('List','Hide','Unhide','Resolve','PruneExpired')][string]$Action='List',
    [string]$ChallengeId,
    [ValidateRange(1,1000)][int]$Maximum=100
)
$ErrorActionPreference='Stop'
if (-not $env:CLOUDFLARE_API_TOKEN) { throw 'Set CLOUDFLARE_API_TOKEN in this process before owner administration.' }
if ($ChallengeId -and $ChallengeId -notmatch '^(?:sf-0(?:0[1-9]|1\d|2[0-4])|[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12})$') { throw 'Supply a valid challenge identifier.' }
if ($Action -in @('Hide','Unhide','Resolve') -and -not $ChallengeId) { throw 'A single ChallengeId is required.' }
$projectRoot=Split-Path $PSScriptRoot -Parent
$hosting=Get-Content -LiteralPath (Join-Path $projectRoot 'docs/hosting.json') -Raw | ConvertFrom-Json
$endpoint="https://api.cloudflare.com/client/v4/accounts/$($hosting.accountId)/d1/database/$($hosting.databaseId)/query"
function Invoke-Query([string]$sql,[array]$parameters) {
    $payload=@{sql=$sql;params=$parameters}|ConvertTo-Json -Depth 4 -Compress
    $result=Invoke-RestMethod -Method Post -Uri $endpoint -Headers @{Authorization=('Bearer '+$env:CLOUDFLARE_API_TOKEN)} -ContentType application/json -Body $payload
    if (-not $result.success -or @($result.result|Where-Object {-not $_.success}).Count) { throw 'D1 did not confirm the moderation action.' }
    return $result.result[0]
}
$now=[DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
switch ($Action) {
    'List' {
        $sql='SELECT r.challenge_id AS challengeId,r.reason,r.note,r.created_at AS reportedAt,m.hidden_at AS hiddenAt FROM signal_reports r LEFT JOIN signal_moderation m ON m.challenge_id=r.challenge_id WHERE r.resolved_at IS NULL'
        $parameters=@()
        if ($ChallengeId) { $sql+=' AND r.challenge_id=?';$parameters=@($ChallengeId) }
        $sql+=' ORDER BY r.created_at LIMIT 50'
        (Invoke-Query $sql $parameters).results
    }
    'Hide' { $result=Invoke-Query 'INSERT INTO signal_moderation(challenge_id,hidden_at) VALUES(?,?) ON CONFLICT(challenge_id) DO NOTHING' @($ChallengeId,$now);Write-Host "Challenge hide confirmed; affected rows: $($result.meta.changes)." }
    'Unhide' { $result=Invoke-Query 'DELETE FROM signal_moderation WHERE challenge_id=?' @($ChallengeId);Write-Host "Challenge unhide confirmed; affected rows: $($result.meta.changes)." }
    'Resolve' { $result=Invoke-Query 'UPDATE signal_reports SET resolved_at=? WHERE challenge_id=? AND resolved_at IS NULL' @($now,$ChallengeId);Write-Host "Reports resolved; affected rows: $($result.meta.changes)." }
    'PruneExpired' {
        $attempts=Invoke-Query 'DELETE FROM signal_attempts WHERE id IN (SELECT id FROM signal_attempts WHERE expires_at<? ORDER BY expires_at LIMIT ?)' @($now,$Maximum)
        $replays=Invoke-Query 'DELETE FROM signal_requests WHERE rowid IN (SELECT rowid FROM signal_requests WHERE created_at<? ORDER BY created_at LIMIT ?)' @($now-7*86400000,$Maximum)
        Write-Host "Expired attempts pruned: $($attempts.meta.changes); expired replay entries: $($replays.meta.changes)."
    }
}
