#requires -version 4.0

if (-not (Test-Path "C:\ProgramData\chocolatey\bin\nuget.exe")) {
	cinst nuget.commandline -y
	nuget update -self
}

Clear-Host
Set-Location $PSScriptRoot
#https://docs.nuget.org/create/creating-and-publishing-a-package
if (-not $env:NUGET_API_KEY) {
	throw "Missing `$Env:NUGET_API_KEY"
}

$lastPackage = dir *.nupkg | Select -Last 1
if (-not $lastPackage) {
	throw "NuGet Package not found"
}
"Publishing $($lastPackage.Name)"
$lastPackage.FullName 
nuget push $lastPackage.FullName $env:NUGET_API_KEY
