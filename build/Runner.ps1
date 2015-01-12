param ($target)

$build = $Env:APPVEYOR_BUILD_NUMBER 

Write-Host "PSake target: $target"
Write-Host "Build number: $build"

$ErrorActionPreference = "Stop"

Import-Module "$PSScriptRoot\modules\psake\psake.psm1"

Invoke-Psake "$PSScriptRoot\Build.ps1" -Parameters @{ "target" = $target; "build" = $build }  