param()
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
if($env:CI -ne 'true' -and $env:GITHUB_ACTIONS -ne 'true'){throw 'Mounted source fetching is restricted to CI.'}
$projectRoot=Split-Path $PSScriptRoot -Parent
$sourcesRoot=Join-Path $projectRoot '.mounted-sources'
$products=@(Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'integrations/mounted-products.json')|ConvertFrom-Json)
function Invoke-MountedGit([string[]]$Arguments){
 $result=@(& git @Arguments 2>&1)
 if($LASTEXITCODE -ne 0){throw ('Git failed: '+($result -join "`n"))}
 return $result
}
$directories=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($product in $products){
 if($product.sourceDirectory -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,79}$' -or !$directories.Add($product.sourceDirectory)){throw 'Mounted source directories must be unique simple folder names.'}
 if($product.repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$' -or $product.ref -notmatch '^[a-f0-9]{40}$'){throw 'Mounted sources require a public GitHub repository and a full lowercase commit SHA.'}
}
New-Item -ItemType Directory -Path $sourcesRoot -Force|Out-Null
foreach($product in $products){
 $destination=Join-Path $sourcesRoot $product.sourceDirectory
 $remote='https://github.com/'+$product.repository+'.git'
 if(Test-Path -LiteralPath $destination){
  if(!(Test-Path -LiteralPath (Join-Path $destination '.git'))){
   if(@(Get-ChildItem -Force -LiteralPath $destination).Count){throw ('Refusing to replace a non-repository source directory: '+$product.sourceDirectory)}
   $null=Invoke-MountedGit -Arguments @('init',$destination)
   $null=Invoke-MountedGit -Arguments @('-C',$destination,'remote','add','origin',$remote)
  }else{
   $top=([string](Invoke-MountedGit -Arguments @('-C',$destination,'rev-parse','--show-toplevel'))).Trim()
   if([IO.Path]::GetFullPath($top) -ne [IO.Path]::GetFullPath($destination)){throw 'The source checkout must be its own repository root.'}
   if(@(Invoke-MountedGit -Arguments @('-C',$destination,'status','--porcelain','--untracked-files=all')).Count){throw ('Refusing to overwrite a dirty source checkout: '+$product.sourceDirectory)}
   $origin=([string](Invoke-MountedGit -Arguments @('-C',$destination,'remote','get-url','origin'))).Trim()
   if($origin.TrimEnd('/') -ne $remote){throw ('The existing source remote differs from the manifest: '+$product.sourceDirectory)}
  }
 }else{
  $null=Invoke-MountedGit -Arguments @('init',$destination)
  $null=Invoke-MountedGit -Arguments @('-C',$destination,'remote','add','origin',$remote)
 }
 $null=Invoke-MountedGit -Arguments @('-C',$destination,'fetch','--no-tags','--depth=1','origin',$product.ref)
 $null=Invoke-MountedGit -Arguments @('-C',$destination,'checkout','--detach',$product.ref)
 $actual=([string](Invoke-MountedGit -Arguments @('-C',$destination,'rev-parse','HEAD'))).Trim()
 if($actual -cne $product.ref){throw ('Fetched source does not match its pinned commit: '+$product.sourceDirectory)}
 Write-Output ('Fetched '+$product.repository+' @ '+$actual)
}
