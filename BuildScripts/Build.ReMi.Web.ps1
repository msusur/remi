param([String]$nugetRepo, [String]$apiKey)

Get-Module | % { if($_.Name -eq "NuGetRemi") { Remove-Module NuGetReMi}}
Import-Module .\NuGetReMi.psm1 -disablenamechecking
Get-Module | % { if($_.Name -eq "GitHelper") { Remove-Module GitHelper}}
Import-Module .\GitHelper.psm1 -disablenamechecking

$nuspecPath = $(Join-Path $(Get-Location) ..\ReMi.Web\.build)
$nuspecName = "ReMi.Web.nuspec"
$nugetConfig = $(Join-Path $(Get-Location) ..\.nuget\NuGet.config)
$outputFolder = $(Join-Path $nuspecPath "temp")

$version = GetNextVersion $nuspecName $nuspecPath $nugetRepo

PublishToFileSystem $(Join-Path $nuspecPath ..\ReMi.Web.UI\ReMi.Web.UI.csproj) $outputFolder

Copy-Item -Recurse $(Join-Path $nuspecPath temp\_PublishedWebsites\ReMi.Web.UI\) $nuspecPath
Rename-Item $(Join-Path $nuspecPath ReMi.Web.UI) content
Remove-Item -Recurse $(Join-Path $nuspecPath temp) -Force
Remove-Item -Recurse $(Join-Path $nuspecPath content\bin) -Force
Remove-Item $(Join-Path $nuspecPath content\packages.config) -Force
Remove-Item $(Join-Path $nuspecPath content\favicon.ico) -Force
Remove-Item -Recurse $(Join-Path $nuspecPath content\content\*) -Force -Exclude images
Remove-Item -Recurse $(Join-Path $nuspecPath content\fonts) -Force
Remove-Item -Recurse $(Join-Path $nuspecPath content\Scripts) -Force

GenerateNuGetPackage $nuspecName $nuspecPath $version

PublishPackage $nugetRepo $nuspecName $nuspecPath $nugetConfig $apiKey

RemoveNuGetPackages $nuspecPath $true

$project = $nuspecName.Substring(0, $nuspecName.LastIndexOf(".nuspec"))
CommitPackage $project $version
