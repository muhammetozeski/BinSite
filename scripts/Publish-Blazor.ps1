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
$appPlaceholder = [regex]::Match($bootstrap, '(?s)<div id="app">.*?</div>').Value
if (-not $appPlaceholder) { throw 'The browser entry point is missing its application mount.' }
$hosting = Get-Content -LiteralPath (Join-Path $projectRoot 'docs/hosting.json') -Raw | ConvertFrom-Json
$siteOrigin = $hosting.siteUrl.TrimEnd('/')
$blazorProducts = @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'catalog') -Filter '*.json' | ForEach-Object {
    Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json | Where-Object { $_.runtime -eq 'blazor' -or ($_.category -eq 'Social' -and $_.stage -ge 3) }
})
$blazorProducts += [pscustomobject]@{
    slug = 'uyelik'; name = 'Creator account'
    description = 'Create or recover your account, manage private cloud projects and review optional membership availability.'
    features = @('Recovery-key access', 'Private cloud storage', 'Membership and billing status')
}
foreach ($product in $blazorProducts) {
    if ($product.slug -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') { throw 'Invalid Blazor product path.' }
    $productDirectory = Join-Path $publicRoot $product.slug
    New-Item -ItemType Directory -Path $productDirectory -Force | Out-Null
    $productTitle = [System.Net.WebUtility]::HtmlEncode($product.name + ' — BinSite')
    $productName = [System.Net.WebUtility]::HtmlEncode($product.name)
    $productDescription = [System.Net.WebUtility]::HtmlEncode($product.description)
    $featureList = @($product.features | ForEach-Object { '<li>' + [System.Net.WebUtility]::HtmlEncode($_) + '</li>' }) -join ''
    $canonicalUrl = [System.Net.WebUtility]::HtmlEncode("$siteOrigin/$($product.slug)/")
    $entryContent = "<div id=""app""><main class=""route-message""><h1>$productName</h1><p>$productDescription</p><ul>$featureList</ul><p role=""status"">Loading application controls.</p><noscript>Enable JavaScript to use this application's interactive controls.</noscript></main></div>"
    $productBootstrap = $bootstrap.Replace('<title>BinSite Social</title>', "<title>$productTitle</title>").Replace($appPlaceholder, $entryContent)
    $metadata = "<meta name=""description"" content=""$productDescription""><link rel=""canonical"" href=""$canonicalUrl""><meta property=""og:title"" content=""$productName""><meta property=""og:description"" content=""$productDescription""><meta property=""og:url"" content=""$canonicalUrl"">"
    $productBootstrap = $productBootstrap.Replace('</head>', $metadata + '</head>')
    [System.IO.File]::WriteAllText((Join-Path $productDirectory 'index.html'), $productBootstrap, [System.Text.UTF8Encoding]::new($false))
}
Write-Host 'Published C# browser runtime and independent product entry points; portfolio homepage preserved.'
