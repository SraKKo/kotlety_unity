param([string]$UnityData='C:\Program Files\Unity\Hub\Editor\6000.3.13f1\Editor\Data')
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
function Assert-Migration($condition,[string]$message){if(-not $condition){throw $message}}
$export=Join-Path $project 'Assets\Resources\Converted'
$data=Get-Content (Join-Path $export 'level.json') -Raw | ConvertFrom-Json
Assert-Migration ($data.spawners.Count -eq 6) 'Expected six wave spawners'
Assert-Migration ($data.actors.Count -eq 9) 'Expected nine interactive actors'
Assert-Migration ($data.colliders.Count -eq 112) 'Expected 112 active colliders'
foreach($spawner in $data.spawners){Assert-Migration ($spawner.points.Count -gt 0) 'Missing spawn points';Assert-Migration ($spawner.waves.Count -gt 0) 'Missing waves'}
foreach($scene in @('MainMenu','Level001','TestLevel')){
 $path=Join-Path $project ('Assets\Scenes\'+$scene+'.unity')
 Assert-Migration (Test-Path $path) "Missing Unity scene: $scene"
 $sceneText=Get-Content $path -Raw
 $meta=Get-Content (Join-Path $project 'Assets\Scripts\MigrationBootstrap.cs.meta') -Raw
 $scriptGuid=[regex]::Match($meta,'guid: ([0-9a-f]{32})').Groups[1].Value
 $sessionMeta=Get-Content (Join-Path $project 'Assets\Scripts\GameSession.cs.meta') -Raw
 $sessionGuid=[regex]::Match($sessionMeta,'guid: ([0-9a-f]{32})').Groups[1].Value
 Assert-Migration ($sceneText.Contains($scriptGuid) -or $sceneText.Contains($sessionGuid)) 'Scene has neither bootstrap nor built game session'
}
$presentation=Get-Content (Join-Path $export 'presentation.json') -Raw | ConvertFrom-Json -Depth 80
Assert-Migration ([Math]::Abs($presentation.bodyRadius-.5) -lt .0001) 'Original player capsule radius changed'
Assert-Migration ([Math]::Abs($presentation.camera.position.y-1.091) -lt .0001) 'Original camera height changed'
Assert-Migration ([Math]::Abs($presentation.weaponFov-110) -lt .0001) 'Original weapon FOV changed'
$testData=Get-Content (Join-Path $export 'test_level.json') -Raw | ConvertFrom-Json -Depth 80
Assert-Migration ($testData.colliders.Count -eq 72 -and $testData.actors.Count -eq 11) 'Incomplete test level export'
Assert-Migration (@($testData.actors | Where-Object kind -eq 'rotating').Count -eq 2) 'Missing rotating test platforms'
Assert-Migration (@($testData.actors | Where-Object kind -eq 'shooting').Count -eq 1) 'Missing test shooter'
Assert-Migration (@($testData.colliders | Where-Object {$_.kind -eq 'mesh' -and -not $_.preserveFaceOrder}).Count -eq 0) 'Test collision winding was not preserved'
$models=@()
foreach($file in Get-ChildItem $export -Filter '*.glb'){
 $stream=[IO.File]::OpenRead($file.FullName);$reader=[IO.BinaryReader]::new($stream)
 try {
  Assert-Migration ($reader.ReadUInt32() -eq 0x46546C67) "Invalid GLB magic: $($file.Name)"
  Assert-Migration ($reader.ReadUInt32() -eq 2) 'Expected glTF 2.0'
  Assert-Migration ($reader.ReadUInt32() -eq $file.Length) 'GLB file length mismatch'
  $length=$reader.ReadUInt32();Assert-Migration ($reader.ReadUInt32() -eq 0x4E4F534A) 'Missing JSON chunk'
  $json=[Text.Encoding]::UTF8.GetString($reader.ReadBytes($length)) | ConvertFrom-Json -Depth 100
  Assert-Migration ($json.meshes.Count -gt 0) "No meshes in $($file.Name)"
  foreach($node in $json.nodes){if($null -ne $node.mesh){Assert-Migration ($node.mesh -lt $json.meshes.Count) 'Invalid mesh reference'}}
  foreach($mesh in $json.meshes){foreach($primitive in $mesh.primitives){Assert-Migration ($null -ne $primitive.attributes.POSITION) 'Mesh lacks positions'}}
  foreach($image in $json.images){Assert-Migration ($null -ne $image.bufferView -or $null -ne $image.uri) 'Missing image data'}
  if($file.BaseName -eq 'RightHandRig'){
   foreach($name in @('melee_idle2','melee_attack2','melee_block','throw_schabowy','throw_chrupiaca')){Assert-Migration ($name -in $json.animations.name) "Missing authored clip: $name"}
  }
  if($file.BaseName -eq 'LeftHandRig'){Assert-Migration ('Armature|hook' -in $json.animations.name) 'Missing hook animation'}
  if($file.BaseName -eq 'SausageLink'){Assert-Migration ($json.meshes.Count -gt 0) 'Missing sausage geometry'}
  foreach($animation in $json.animations){Assert-Migration ($animation.channels.Count -gt 0) 'Empty animation'}
  $models+=[ordered]@{name=$file.BaseName;meshes=$json.meshes.Count;nodes=$json.nodes.Count;images=$json.images.Count;animations=$json.animations.Count;skins=$json.skins.Count}
 }finally{$reader.Dispose();$stream.Dispose()}
}
Assert-Migration ($models.Count -eq 26) 'Expected 26 GLB exports'
$report=[ordered]@{colliders=$data.colliders.Count;actors=$data.actors.Count;spawners=$data.spawners.Count;models=$models;runtimeCompilation='not run';editorCompilation='not run';playMode='not run by this static check'}
if(Test-Path (Join-Path $UnityData 'DotNetSdkRoslyn\csc.dll')){
 $refs=@(Get-ChildItem (Join-Path $UnityData 'Managed\UnityEngine') -Filter '*.dll' | ForEach-Object {'-r:"'+$_.FullName+'"'})
 $refs+=Get-ChildItem (Join-Path $UnityData 'NetStandard\ref\2.1.0') -Filter '*.dll' | ForEach-Object {'-r:"'+$_.FullName+'"'}
 $refs+=Get-ChildItem (Join-Path $UnityData 'NetStandard\compat\2.1.0\shims\netfx') -Filter '*.dll' | ForEach-Object {'-r:"'+$_.FullName+'"'}
 $runtime=Join-Path $PSScriptRoot 'RetyKotlety.dll'
 $arguments=@('-nologo','-target:library','-nostdlib+','-langversion:9.0',('-out:"'+$runtime+'"'))+$refs+(Get-ChildItem (Join-Path $project 'Assets\Scripts') -Filter '*.cs' | ForEach-Object {'"'+$_.FullName+'"'})
 $response=Join-Path $PSScriptRoot 'runtime.rsp';$arguments | Set-Content $response
 & (Join-Path $UnityData 'NetCoreRuntime\dotnet.exe') (Join-Path $UnityData 'DotNetSdkRoslyn\csc.dll') "@$response"
 Assert-Migration ($LASTEXITCODE -eq 0) 'Runtime C# compilation failed';$report.runtimeCompilation='passed against installed Unity assemblies'
 $editorRefs=@(Get-ChildItem (Join-Path $UnityData 'Managed') -Filter 'UnityEditor*.dll' -Recurse | ForEach-Object {'-r:"'+$_.FullName+'"'})
 $editorArguments=@('-nologo','-target:library','-nostdlib+','-langversion:9.0',('-out:"'+$PSScriptRoot+'\RetyKotlety.Editor.dll"'),('-r:"'+$runtime+'"'))+$refs+$editorRefs+(Get-ChildItem (Join-Path $project 'Assets\Editor') -Filter '*.cs' | ForEach-Object {'"'+$_.FullName+'"'})
 $editorResponse=Join-Path $PSScriptRoot 'editor.rsp';$editorArguments | Set-Content $editorResponse
 & (Join-Path $UnityData 'NetCoreRuntime\dotnet.exe') (Join-Path $UnityData 'DotNetSdkRoslyn\csc.dll') "@$editorResponse"
 Assert-Migration ($LASTEXITCODE -eq 0) 'Editor C# compilation failed';$report.editorCompilation='passed against installed Unity assemblies'
 $movementArguments=@('-nologo','-target:exe','-nostdlib+','-langversion:9.0',('-out:"'+$PSScriptRoot+'\MovementTests.exe"'),('-r:"'+$runtime+'"'))+$refs+('"'+$PSScriptRoot+'\MovementTests.cs"')
 $movementResponse=Join-Path $PSScriptRoot 'movement.rsp';$movementArguments | Set-Content $movementResponse
 & (Join-Path $UnityData 'NetCoreRuntime\dotnet.exe') (Join-Path $UnityData 'DotNetSdkRoslyn\csc.dll') "@$movementResponse"
 Assert-Migration ($LASTEXITCODE -eq 0) 'Movement regression test compilation failed'
 $oldMonoPath=$env:MONO_PATH
 try {
  $env:MONO_PATH=Join-Path $UnityData 'Managed\UnityEngine'
  & (Join-Path $UnityData 'MonoBleedingEdge\bin\mono.exe') (Join-Path $PSScriptRoot 'MovementTests.exe') | Tee-Object (Join-Path $PSScriptRoot 'movement-results.txt')
  Assert-Migration ($LASTEXITCODE -eq 0) 'Movement regression checks failed'
  $report.movementRegression='10 checks passed'
 } finally {$env:MONO_PATH=$oldMonoPath}
}
$playErrors=Join-Path $PSScriptRoot 'playmode-errors.txt'
$playResults=Join-Path $PSScriptRoot 'playmode-results.txt'
if((Test-Path $playErrors) -and (Test-Path $playResults)){
 $checks=@(Get-Content $playResults)
 if((Get-Item $playErrors).Length -eq 0 -and ($checks -match 'No runtime errors').Count -gt 0){$report.playMode='passed in Unity 6000.3.13f1';$report.playModeChecks=$checks.Count}
}
$testErrors=Join-Path $PSScriptRoot 'test-level-errors.txt'
$testResults=Join-Path $PSScriptRoot 'test-level-results.txt'
if((Test-Path $testErrors) -and (Test-Path $testResults)){
 $testChecks=@(Get-Content $testResults)
 if((Get-Item $testErrors).Length -eq 0 -and ($testChecks -match 'No runtime errors').Count -gt 0){$report.testLevelPlayMode='passed in Unity 6000.3.13f1';$report.testLevelChecks=$testChecks.Count}
}
$combatErrors=Join-Path $PSScriptRoot 'combat-hud-errors.txt'
$combatResults=Join-Path $PSScriptRoot 'combat-hud-results.txt'
if((Test-Path $combatErrors) -and (Test-Path $combatResults)){
 $combatChecks=@(Get-Content $combatResults)
 if((Get-Item $combatErrors).Length -eq 0 -and ($combatChecks -match 'No runtime errors').Count -gt 0){$report.combatHudPlayMode='passed in Unity 6000.3.13f1';$report.combatHudChecks=$combatChecks.Count}
}
$arenaErrors=Join-Path $PSScriptRoot 'arena-movement-errors.txt'
$arenaResults=Join-Path $PSScriptRoot 'arena-movement-results.txt'
if((Test-Path $arenaErrors) -and (Test-Path $arenaResults)){
 $arenaChecks=@(Get-Content $arenaResults)
 if((Get-Item $arenaErrors).Length -eq 0 -and ($arenaChecks -match 'No runtime movement errors').Count -gt 0){$report.arenaMovementPlayMode='passed in Unity 6000.3.13f1';$report.arenaMovementChecks=$arenaChecks.Count}
 $arenaEditorIssues=Join-Path $PSScriptRoot 'arena-movement-editor-issues.txt'
 if((Test-Path $arenaEditorIssues) -and (Get-Item $arenaEditorIssues).Length -gt 0){$report.arenaMovementEditorNote='Unity Search startup exception; full evidence in arena-movement-editor-issues.txt'}
}
$report | ConvertTo-Json -Depth 8 | Set-Content -Encoding utf8 (Join-Path $PSScriptRoot 'report.json')
$report | ConvertTo-Json -Depth 8
