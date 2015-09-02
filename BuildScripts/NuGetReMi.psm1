function GenerateNuGetPackage([String]$nuspecName, [String]$nuspecPath, [String]$version)
{
    Write-Host 'GenerateNuGetPackage - started'

    $nuspecWithPath = $(Join-Path $nuspecPath $nuspecName)

    if(!(Test-Path $nuspecWithPath))
    {
        throw "Nuspec file $nuspecName is missing at $nuspecPath"
    }

    ChangeVersionInNuspec $nuspecWithPath $version  

    ..\.nuget\nuget.exe pack $nuspecWithPath -o $nuspecPath

    Write-Host 'GenerateNuGetPackage - finished'

	if (!$?){
		throw "Failed to generate nuget package"
	}
}

function GetNextVersion([String]$nuspecName, [String]$nuspecPath, [String]$nugetSource)
{
    Write-Host 'GetNextVersion -started'

    $nuspecWithPath = $(Join-Path $nuspecPath $nuspecName)

    if(!(Test-Path $nuspecWithPath))
    {
        throw "Nuspec file $nuspecName is missing at $nuspecPath"
    }

    $packageName = $(GetPropertyFromNuspec $nuspecWithPath "id")

    $lastPackage = ..\.nuget\nuget.exe list $packageName -source $nugetSource
    $packageVersion = $(GetPropertyFromNuspec $nuspecWithPath "version")

    $versionPattern = "\d+.\d+.\d+.(\d+)$"
    $replacePattern = "\d+$"
    $buildVersion = "0"


    if(($lastPackage[$lastPackage.Length - 1].Contains("$packageName")) -and ($lastPackage[$lastPackage.Length - 1] -match $versionPattern))
    {
        $buildVersion = [String]([int]$matches[1] + 1)
    }
    if(-not ($packageVersion -match $versionPattern))
    {
        $packageVersion = "1.0.0.0"
    }

    $version = $packageVersion -replace $replacePattern, $buildVersion
    Write-Host "New version $version"
    Write-Host "GetNextVersion - finished"

	if (!$?){
		throw "Failed to get next version of nuget package"
	}

    return $version
}

function GetPropertyFromNuspec([String]$nuspecWithPath, [String]$propertyName)
{
    Write-Host 'GetPropertyFromNuspec - started'
    [Xml]$nuspec = Get-Content $nuspecWithPath
    $value = $nuspec.package.metadata.$propertyName
    Write-Host 'GetPropertyFromNuspec - finished'
    Write-Host $propertyName " equals " $value
    return $value    
}

function ChangeVersionInNuspec([String]$nuspecWithPath, [String]$version)
{
    Write-Host 'ChangeVersionInNuspec - started'
    [Xml]$nuspec = Get-Content $nuspecWithPath
    $nuspec.package.metadata.version = $version
    $nuspec.Save("$nuspecWithPath")
    Write-Host 'ChangeVersionInNuspec - finished'

	if (!$?){
		throw "Failed to update package version"
	}
}

function ChangeVersionInAssemblies([String]$version, [String]$solutionPath)
{
    Write-Host 'ChangeVersionInAssemblies -started'

    Get-ChildItem -Path $solutionPath -Recurse |? { $_.Name -eq "AssemblyInfo.cs" } | Update-SourceVersion $version

    Write-Host "ChangeVersionInAssemblies - finished"
}

function Update-SourceVersion
{
  Param ([string]$Version)
  $NewVersion = 'AssemblyVersion("' + $Version + '")';
  $NewFileVersion = 'AssemblyFileVersion("' + $Version + '")';

  foreach ($o in $input) 
  {
    Write-Host "Updating " $o.FullName
    $TmpFile = $o.FullName + ".tmp"

     get-content $o.FullName | 
        %{$_ -replace 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewVersion } |
        %{$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewFileVersion }  > $TmpFile

     move-item $TmpFile $o.FullName -force
  }
}

function PublishPackage([String]$publishDestination, [String]$nuspecName, [String]$nuspecPath, [String]$configFile, [String]$apiKey)
{
    Write-Host 'PublishPackage - started'
    $nuspecWithPath = $(Join-Path $nuspecPath $nuspecName)

    if(!(Test-Path $nuspecWithPath))
    {
        throw "Nuspec file $nuspecName is missing at $nuspecPath"
    }

    $packageName = $(GetPropertyFromNuspec $nuspecWithPath "id")
    $packageVersion = $(GetPropertyFromNuspec $nuspecWithPath "version")
    
    $nupkg = "$packageName.$packageVersion.nupkg"
   
    $packagePath = $(Join-Path $nuspecPath $nupkg)

    $command = "..\.nuget\nuget.exe push $packagePath -source $publishDestination -apikey $apiKey" 
    
    Invoke-Expression $command

    Write-Host 'PublishPackage - finished'

	if (!$?){
		throw "Failed to publish nuget package to artifactory"
	}
}

function CleanupPackageContent([String]$contentFolder, $matchPattern)
{
    Write-Host 'CleanupPackageContent - started'

    Push-Location $contentFolder

    Get-ChildItem "*" -Exclude $matchPattern | %{Remove-Item $_}
    Get-ChildItem | ?{ $_.PSIsContainer } | %{Remove-Item $_}

    Pop-Location

    Write-Host 'CleanupPackageContent - finished'

	if (!$?){
		throw "Failed to clean up after creating package"
	}
}

function RemoveNuGetPackages([String]$nuspecPath, [Parameter(Mandatory=$false)][String]$removeContent=$false)
{
    Write-Host 'RemoveNuGetPackages - started'

    Push-Location $nuspecPath

    Get-Childitem | ?{($_.Name -notlike "*.nuspec") -and ($_.Name -notlike "content" -or $removeContent -like $true)} | %{Remove-Item $_ -r}

    Pop-Location

    Write-Host 'RemoveNuGetPackages - finished'

	if (!$?){
		throw "Failed to clean up after creating package"
	}
}

function PublishToFileSystem([String]$publishingProjectPath, [String]$publishUrl, [Parameter(Mandatory=$false)][String]$mode="Release")
{
    Write-Host 'PublishToFileSystem - started'
    if(!(Test-Path $publishingProjectPath))
    {
        throw "Project $publishingProjectPath was not found"
    }

    if(!(Test-Path $publishUrl))
    {
        New-Item -ItemType directory -path $publishUrl
    }

    try 
    {
        msbuild $publishingProjectPath /p:Configuration=$mode /p:Platform=AnyCPU /p:OutputPath=$publishUrl /p:DebugSymbols=false /p:DebugType=None

        if ($LastExitCode -ne 0)
        {
            throw "Publish to filesystem $publishingProjectPath to $publishUrl task for $solutionName failed"
        }
    } Catch [Exception] {
        write-host $_.Exception.GetType().FullName
        write-host $_.Exception.Message
        throw $_
    }
    Write-Host 'PublishToFileSystem - finished'

	if (!$?){
		throw "Failed to publish nuget package to system files"
	}
}
