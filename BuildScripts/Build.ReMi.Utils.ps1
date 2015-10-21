param([String]$nugetRepo, [String]$apiKey)

Get-Module | % { if($_.Name -eq "NuGetRemi") { Remove-Module NuGetReMi}}
Import-Module .\NuGetReMi.psm1 -disablenamechecking
Get-Module | % { if($_.Name -eq "GitHelper") { Remove-Module GitHelper}}
Import-Module .\GitHelper.psm1 -disablenamechecking

$nuspecPath = $(Join-Path $(Get-Location) ..\ReMi.Utils\.build)
$nuspecName = "ReMi.Utils.nuspec"
$nugetConfig = $(Join-Path $(Get-Location) ..\.nuget\NuGet.config)
$outputFolder = $(Join-Path $nuspecPath "lib\\net45")
$matchPattern = ("ReMi.Common.Utils.dll", "ReMi.Common.Logging.dll", "ReMi.Plugin.Common.dll")

$version = GetNextVersion $nuspecName $nuspecPath $nugetRepo

ChangeVersionInAssemblies $version $(Join-Path $nuspecPath ..\)

PublishToFileSystem $(Join-Path $nuspecPath ..\ReMi.Common.Logging\ReMi.Common.Logging.csproj) $outputFolder
PublishToFileSystem $(Join-Path $nuspecPath ..\ReMi.Common.Utils\ReMi.Common.Utils.csproj) $outputFolder
PublishToFileSystem $(Join-Path $nuspecPath ..\ReMi.Plugin.Common\ReMi.Plugin.Common.csproj) $outputFolder

CleanupPackageContent $outputFolder $matchPattern

GenerateNuGetPackage $nuspecName $nuspecPath $version

PublishPackage $nugetRepo $nuspecName $nuspecPath $nugetConfig $apiKey

RemoveNuGetPackages $nuspecPath

$project = $nuspecName.Substring(0, $nuspecName.LastIndexOf(".nuspec"))
CommitPackage $project $version
