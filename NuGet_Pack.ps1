#requires -version 4.0

if (-not (Test-Path "C:\ProgramData\chocolatey\bin\nuget.exe")) {
	cinst nuget.commandline -y
	nuget update -self
}

Clear-Host
Set-Location $PSScriptRoot
"Packaging nugets..."
dir *.nuspec | % {
	$_.Name
	nuget pack $_.Name
}
