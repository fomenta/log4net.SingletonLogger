#requires -version 4.0

if (-not (Test-Path "C:\ProgramData\chocolatey\bin\nuget.exe")) {
	cinst nuget.commandline -y
	nuget update -self
}

Clear-Host
$nugetTargetFolder = "$PSScriptRoot\releases"
Set-Location $nugetTargetFolder
#https://docs.nuget.org/create/creating-and-publishing-a-package
if (-not $env:NUGET_API_KEY) { throw "Missing `$Env:NUGET_API_KEY" }

$lastPackage = dir "$nugetTargetFolder\*.nupkg" | Select -Last 1
if (-not $lastPackage) { throw "No NuGet Package found to publish" }

"Publishing $($lastPackage.Name)"
nuget push $lastPackage.FullName $env:NUGET_API_KEY
