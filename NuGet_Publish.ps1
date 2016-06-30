#requires -version 4.0

if (-not (Test-Path "C:\ProgramData\chocolatey\bin\nuget.exe")) {
	cinst nuget.commandline -y
	nuget update -self
}

Clear-Host
$nugetTargetFolder = "$PSScriptRoot\releases"
Set-Location $nugetTargetFolder
#https://docs.nuget.org/create/creating-and-publishing-a-package
if (-not $env:NUGET_API_KEY) { throw "Missing `$Env:NUGET_API_KEY. Get one at https://www.nuget.org/account - To create, run: [Environment]::SetEnvironmentVariable('NUGET_API_KEY', 'a0830ebb-d6ac-4556-953e-3075366bcf31', 'User')" }

$lastPackage = dir "$nugetTargetFolder\*.nupkg" | Select -Last 1
if (-not $lastPackage) { throw "No NuGet Package found to publish" }

"Publishing $($lastPackage.Name)"
#$ArgList = "push", $lastPackage.FullName, $env:NUGET_API_KEY, "-Source", "https://www.nuget.org/api/v2/package"
$ArgList = "push", $lastPackage.FullName, $env:NUGET_API_KEY, "-Source", "nuget.org"
"nuget $ArgList"
nuget $ArgList
