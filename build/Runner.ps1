param ($target)

$version = $Env:APPVEYOR_BUILD_VERSION

Write-Host "PSake target: $target"
Write-Host "Build version: $version"

$ErrorActionPreference = "Stop"

Import-Module "$PSScriptRoot\modules\psake\psake.psm1"

Invoke-Psake "$PSScriptRoot\Build.ps1" -Parameters @{ "target" = $target; "version" = $version }  