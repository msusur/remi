param([String]$nugetRepo, [String]$apiKey)

Get-Module | % { if($_.Name -eq "NuGetRemi") { Remove-Module NuGetReMi}}
Import-Module .\NuGetReMi.psm1 -disablenamechecking

$nuspecPath = $(Join-Path $(Get-Location) ..\Plugins\Email\.build)
$nuspecName = "ReMi.Plugin.Email.nuspec"
$nugetConfig = $(Join-Path $(Get-Location) ..\.nuget\NuGet.config)
$outputFolder = $(Join-Path $nuspecPath "lib\\net45")
$matchPattern = ("ReMi.Plugin.Email.dll", "ReMi.Plugin.EmailMock.dll")

$version = GetNextVersion $nuspecName $nuspecPath $nugetRepo

ChangeVersionInAssemblies $version ..\

PublishToFileSystem $(Join-Path $nuspecPath ..\ReMi.Plugin.EmailMock\ReMi.Plugin.EmailMock.csproj) $outputFolder

CleanupPackageContent $outputFolder $matchPattern

GenerateNuGetPackage $nuspecName $nuspecPath $version

PublishPackage $nugetRepo $nuspecName $nuspecPath $nugetConfig $apiKey

RemoveNuGetPackages $nuspecPath

