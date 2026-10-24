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
    if ($item.Name -in @('index.html', 'index.html.br', 'index.html.gz')) { continue }
    Copy-Item -LiteralPath $item.FullName -Destination $publicRoot -Recurse -Force
}
$bootstrap = [System.IO.File]::ReadAllText((Join-Path $webRoot 'index.html'))
$blazorProducts = @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'catalog') -Filter '*.json' | ForEach-Object {
    Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json | Where-Object { $_.category -eq 'Social' -and $_.stage -ge 3 }
})
$blazorProducts += [pscustomobject]@{ slug = 'uyelik'; name = 'Creator account' }
foreach ($product in $blazorProducts) {
    if ($product.slug -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') { throw 'Invalid Blazor product path.' }
    $productDirectory = Join-Path $publicRoot $product.slug
    New-Item -ItemType Directory -Path $productDirectory -Force | Out-Null
    $productTitle = [System.Net.WebUtility]::HtmlEncode($product.name + ' — BinSite')
    $productBootstrap = [regex]::Replace($bootstrap, '<title>.*?</title>', "<title>$productTitle</title>")
    [System.IO.File]::WriteAllText((Join-Path $productDirectory 'index.html'), $productBootstrap, [System.Text.UTF8Encoding]::new($false))
}
Write-Host 'Published C# browser runtime and independent social entry points; portfolio homepage preserved.'
