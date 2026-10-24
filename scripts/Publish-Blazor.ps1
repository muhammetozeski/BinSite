param([switch]$NoRestore)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$sourceProject = Join-Path $projectRoot 'src/BinSite.Social/BinSite.Social.csproj'
$publishRoot = Join-Path $projectRoot 'publish/Social'
$publicRoot = Join-Path $projectRoot 'public'
if (-not (Test-Path -LiteralPath $sourceProject)) { throw 'The Blazor project has not been created yet.' }
$publishArgs = @('publish', $sourceProject, '-c', 'Release', '-o', $publishRoot, '-p:WasmEnableWebcil=true')
if ($NoRestore) { $publishArgs += '--no-restore' }
& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) { throw 'Blazor publication failed.' }
$webRoot = Join-Path $publishRoot 'wwwroot'
foreach ($item in Get-ChildItem -LiteralPath $webRoot) {
    if ($item.Name -eq 'index.html') { continue }
    Copy-Item -LiteralPath $item.FullName -Destination $publicRoot -Recurse -Force
}
$bootstrap = [System.IO.File]::ReadAllText((Join-Path $webRoot 'index.html'))
foreach ($slug in @('hikaye-agaci', 'fikir-duellosu', 'zaman-kapsulu', 'uyelik')) {
    $productDirectory = Join-Path $publicRoot $slug
    New-Item -ItemType Directory -Path $productDirectory -Force | Out-Null
    [System.IO.File]::WriteAllText((Join-Path $productDirectory 'index.html'), $bootstrap, [System.Text.UTF8Encoding]::new($false))
}
Write-Host 'Published C# browser runtime and independent social entry points; portfolio homepage preserved.'
