#requires -version 4.0

if (-not (Test-Path "C:\ProgramData\chocolatey\bin\nuget.exe")) {
	cinst nuget.commandline -y
	nuget update -self
}

Clear-Host
Set-Location $PSScriptRoot

# create target if not found
$nugetTargetFolder = "$PSScriptRoot\releases"
if (-not (Test-Path $nugetTargetFolder -PathType Container)) { md $nugetTargetFolder | Out-Null }

"Packaging nugets..."
dir *.nuspec | % {
	$_.Name
	nuget pack $_.Name -OutputDirectory "$nugetTargetFolder"
}
