param([String]$nugetRepo, [String]$apiKey)

Get-Module | % { if($_.Name -eq "NuGetRemi") { Remove-Module NuGetReMi}}
Import-Module .\NuGetReMi.psm1 -disablenamechecking

$nuspecPath = $(Join-Path $(Get-Location) ..\Plugins\GO\.build)
$nuspecName = "ReMi.Plugin.Go.nuspec"
$nugetConfig = $(Join-Path $(Get-Location) ..\.nuget\NuGet.config)
$outputFolder = $(Join-Path $nuspecPath "lib\\net45")
$matchPattern = ("ReMi.Plugin.Go.dll")

$version = GetNextVersion $nuspecName $nuspecPath $nugetRepo

ChangeVersionInAssemblies $version ..\

PublishToFileSystem $(Join-Path $nuspecPath ..\ReMi.Plugin.Go\ReMi.Plugin.Go.csproj) $outputFolder

CleanupPackageContent $outputFolder $matchPattern

GenerateNuGetPackage $nuspecName $nuspecPath $version

PublishPackage $nugetRepo $nuspecName $nuspecPath $nugetConfig $apiKey

RemoveNuGetPackages $nuspecPath

