param(
 [string]$SourcesRoot='',
 [switch]$NoRestore,
 [switch]$AllowWorkingTree
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$projectRoot=Split-Path $PSScriptRoot -Parent
if(!$SourcesRoot){$SourcesRoot=Split-Path $projectRoot -Parent}
$SourcesRoot=[IO.Path]::GetFullPath($SourcesRoot)
if($AllowWorkingTree -and ($env:CI -eq 'true' -or $env:GITHUB_ACTIONS -eq 'true')){throw 'Working-tree overrides are only permitted for local validation.'}
$products=@(Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'integrations/mounted-products.json')|ConvertFrom-Json)
$pathComparison=if([OperatingSystem]::IsWindows()){[StringComparison]::OrdinalIgnoreCase}else{[StringComparison]::Ordinal}
function Within-MountedRoot([string]$Root,[string]$Relative){
 if(!$Relative -or [IO.Path]::IsPathRooted($Relative)){throw 'Mounted manifest paths must be relative.'}
 $absolute=[IO.Path]::GetFullPath((Join-Path $Root $Relative))
 $prefix=[IO.Path]::GetFullPath($Root).TrimEnd([IO.Path]::DirectorySeparatorChar,[IO.Path]::AltDirectorySeparatorChar)+[IO.Path]::DirectorySeparatorChar
 if(!$absolute.StartsWith($prefix,$pathComparison)){throw 'Mounted manifest path escapes its expected directory.'}
 return $absolute
}
function Read-MountedGit([string[]]$Arguments){
 $result=@(& git @Arguments 2>&1)
 if($LASTEXITCODE -ne 0){throw ('Source verification failed: '+($result -join "`n"))}
 return $result
}
$keys=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$slugs=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$sourceNames=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$jobs=@()
foreach($product in $products){
 if($product.key -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$' -or !$keys.Add($product.key) -or $product.slug -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$' -or !$slugs.Add($product.slug)){throw 'Mounted keys and slugs must be unique simple names.'}
 if($product.sourceDirectory -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,79}$' -or !$sourceNames.Add($product.sourceDirectory) -or $product.ref -notmatch '^[a-f0-9]{40}$'){throw 'Mounted sources require unique folders and full lowercase commit SHAs.'}
 $source=Within-MountedRoot $SourcesRoot $product.sourceDirectory
 if(!(Test-Path -LiteralPath (Join-Path $source '.git'))){throw ('Missing source repository: '+$product.sourceDirectory)}
 $top=([string](Read-MountedGit -Arguments @('-C',$source,'rev-parse','--show-toplevel'))).Trim()
 if([IO.Path]::GetFullPath($top) -ne [IO.Path]::GetFullPath($source)){throw 'Each mounted source must be its own repository root.'}
 $actual=([string](Read-MountedGit -Arguments @('-C',$source,'rev-parse','HEAD'))).Trim()
 $dirty=@(Read-MountedGit -Arguments @('-C',$source,'status','--porcelain','--untracked-files=all')).Count -gt 0
 if(!$AllowWorkingTree -and ($actual -cne $product.ref -or $dirty)){throw ('Source must be clean at its pinned ref: '+$product.sourceDirectory)}
 if($AllowWorkingTree){Write-Output ('Local working-tree validation: '+$product.sourceDirectory+' @ '+$actual)}
 $publish=Within-MountedRoot $source $product.publishScript
 if(!(Test-Path -LiteralPath $publish -PathType Leaf)){throw ('Missing publish script: '+$product.publishScript)}
 $files=@()
 foreach($relative in $product.backendFiles){
  $file=Within-MountedRoot $source $relative
  if(!(Test-Path -LiteralPath $file -PathType Leaf)){throw ('Missing explicit backend file: '+$product.key+'/'+$relative)}
  $files+=@{Relative=$relative;Path=$file}
 }
 $arguments=@{BasePath='/'+$product.slug+'/';Destination=(Join-Path $projectRoot ('public/'+$product.slug))}
 if($NoRestore){$arguments.NoRestore=$true}
 foreach($argument in $product.publishArguments.PSObject.Properties){
  if($argument.Name -in @('BasePath','Destination','NoRestore')){throw 'publishArguments cannot replace the integration base, destination or restore choice.'}
  $arguments[$argument.Name]=$argument.Value
 }
 $jobs+=@{Product=$product;Source=$source;Publish=$publish;Arguments=$arguments;Files=$files}
}
$begin='# BEGIN BINSITE MOUNTED PRODUCTS'
$end='# END BINSITE MOUNTED PRODUCTS'
function Read-UnmountedMetadata([string]$Path){
 $original=if(Test-Path -LiteralPath $Path){@(Get-Content -LiteralPath $Path)}else{@()}
 $base=[Collections.Generic.List[string]]::new();$inside=$false
 foreach($line in $original){
  if($line -ceq $begin){if($inside){throw 'Malformed mounted metadata block.'};$inside=$true;continue}
  if($line -ceq $end){if(!$inside){throw 'Malformed mounted metadata block.'};$inside=$false;continue}
  if(!$inside){$base.Add($line)}
 }
 if($inside){throw 'An existing mounted metadata block has no closing marker.'}
 while($base.Count -gt 0 -and !$base[$base.Count-1].Trim()){$base.RemoveAt($base.Count-1)}
 return $base.ToArray()
}
$inherited=[Collections.Generic.Dictionary[string,Collections.Generic.HashSet[string]]]::new([StringComparer]::OrdinalIgnoreCase)
$mainRule=''
foreach($line in @(Read-UnmountedMetadata (Join-Path $projectRoot 'public/_headers'))){
 if(!$line.Trim() -or $line.TrimStart().StartsWith('#')){continue}
 if(!$line.StartsWith(' ') -and !$line.StartsWith("`t")){$mainRule=$line.Trim();continue}
 if($mainRule -ne '/*'){continue}
 $colon=$line.IndexOf(':');if($colon -lt 1){continue}
 $field=$line.Substring(0,$colon).Trim();$value=$line.Substring($colon+1).Trim()
 if(!$inherited.ContainsKey($field)){$inherited[$field]=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)}
 $null=$inherited[$field].Add($value)
}
function Remove-InheritedHeaders([string[]]$Lines){
 $result=[Collections.Generic.List[string]]::new();$group=[Collections.Generic.List[string]]::new();$count=0;$needsFrameDeny=$false;$hasFrameDeny=$false
 foreach($line in $Lines){
  if($line.StartsWith('/')){
   if($needsFrameDeny -and !$hasFrameDeny){$group.Add("  Content-Security-Policy: frame-ancestors 'none'");$count++}
   if($count){$result.AddRange($group);$result.Add('')}
   $group.Clear();$group.Add($line);$count=0;$needsFrameDeny=$false;$hasFrameDeny=$false;continue
  }
  if(!$line.Trim() -or $line.TrimStart().StartsWith('#')){if($group.Count){$group.Add($line)};continue}
  $colon=$line.IndexOf(':');if($colon -lt 1 -or !$group.Count){throw 'Malformed child header rule.'}
  $field=$line.Substring(0,$colon).Trim();$value=$line.Substring($colon+1).Trim()
  if($field -ieq 'Content-Security-Policy'){
   foreach($directive in $value.Split(';')){
    $tokens=$directive.Trim().Split([char[]]@(' ',"`t"),[StringSplitOptions]::RemoveEmptyEntries)
    if($tokens.Count -eq 2 -and $tokens[0] -ieq 'frame-ancestors' -and $tokens[1] -ceq "'none'"){$hasFrameDeny=$true}
   }
  }
  if($field -ieq 'X-Frame-Options' -and $inherited.ContainsKey($field)){
   $globalValues=@($inherited[$field])
   if($globalValues.Count -ne 1 -or $globalValues[0] -notin @('SAMEORIGIN','DENY')){throw 'Unexpected global X-Frame-Options combination.'}
   if($value -ieq $globalValues[0]){continue}
   if($globalValues[0] -ieq 'SAMEORIGIN' -and $value -ieq 'DENY'){$needsFrameDeny=$true;continue}
   throw ('Unexpected scoped X-Frame-Options combination: '+$globalValues[0]+' / '+$value)
  }
  # Pages combines repeated header values; identical global defaults must be inherited once.
  if($inherited.ContainsKey($field) -and $inherited[$field].Contains($value)){continue}
  $group.Add($line);$count++
 }
 if($needsFrameDeny -and !$hasFrameDeny){$group.Add("  Content-Security-Policy: frame-ancestors 'none'");$count++}
 if($count){$result.AddRange($group)}
 return $result.ToArray()
}
$metadata=@{_headers=[Collections.Generic.List[string]]::new();_redirects=[Collections.Generic.List[string]]::new()}
foreach($job in $jobs){
 $product=$job.Product;$parameters=$job.Arguments
 Write-Output ('Publishing mounted product: '+$product.slug)
 & $job.Publish @parameters
 if($LASTEXITCODE -ne 0){throw ('Mounted publication failed: '+$product.slug)}
 $destination=$parameters.Destination
 if(!(Test-Path -LiteralPath (Join-Path $destination 'index.html') -PathType Leaf)){throw ('Mounted publication has no entry page: '+$product.slug)}
 $backend=Join-Path $projectRoot ('integrations/mounted/'+$product.key)
 foreach($file in $job.Files){
  $target=Within-MountedRoot $backend $file.Relative
  New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force|Out-Null
  Copy-Item -LiteralPath $file.Path -Destination $target -Force
 }
 foreach($name in @('_headers','_redirects')){
  $file=Join-Path $destination $name
  if(!(Test-Path -LiteralPath $file -PathType Leaf)){continue}
  $lines=@(Get-Content -LiteralPath $file)
  foreach($line in $lines){
   if(!$line.Trim() -or $line.TrimStart().StartsWith('#')){continue}
   if($name -eq '_headers' -and !$line.StartsWith(' ') -and !$line.StartsWith("`t")){
    if(!$line.StartsWith('/'+$product.slug+'/') -and $line -ne '/'+$product.slug){throw ('Child header rule escapes its mount: '+$product.slug)}
   }
   if($name -eq '_redirects'){
    $parts=$line.Trim().Split([char[]]@(' ',"`t"),[StringSplitOptions]::RemoveEmptyEntries)
    if($parts.Count -lt 2 -or (!$parts[0].StartsWith('/'+$product.slug+'/') -and $parts[0] -ne '/'+$product.slug)){throw ('Child redirect source escapes its mount: '+$product.slug)}
    if($parts[1].StartsWith('/') -and !$parts[1].StartsWith('/'+$product.slug+'/') -and $parts[1] -ne '/'+$product.slug){throw ('Child redirect destination escapes its mount: '+$product.slug)}
   }
  }
  if($name -eq '_headers'){$lines=@(Remove-InheritedHeaders -Lines $lines)}
  if($lines.Count){$metadata[$name].Add('# '+$product.slug);$metadata[$name].AddRange([string[]]$lines);$metadata[$name].Add('')}
 }
}
foreach($name in @('_headers','_redirects')){
 $path=Join-Path $projectRoot ('public/'+$name)
 $base=[Collections.Generic.List[string]]::new();$base.AddRange([string[]]@(Read-UnmountedMetadata $path))
 if($metadata[$name].Count){if($base.Count){$base.Add('')};$base.Add($begin);$base.AddRange($metadata[$name]);$base.Add($end)}
 if($base.Count -gt 0 -or (Test-Path -LiteralPath $path)){[IO.File]::WriteAllText($path,($base -join "`n")+"`n",[Text.UTF8Encoding]::new($false))}
}
$routes=[ordered]@{version=1;include=@('/api/*')+@($products|ForEach-Object {'/'+$_.slug+'/api/*'});exclude=@()}
[IO.File]::WriteAllText((Join-Path $projectRoot 'public/_routes.json'),($routes|ConvertTo-Json -Compress)+"`n",[Text.UTF8Encoding]::new($false))
Write-Output ('Prepared '+$products.Count+' pinned mounted products and merged scoped hosting metadata.')
