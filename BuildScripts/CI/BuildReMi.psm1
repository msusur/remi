
function BuildSolution([Parameter(Mandatory=$false)][String]$solutionName, [String]$mode="Release")
{
    $solution = GetSolutionFileInfo $solutionName
    $solutionFile = $solution.FullName
    
    ..\..\.nuget\nuget.exe install ..\..\.nuget\packages.config -o ..\..\packages

    msbuild /t:Clean /p:Configuration=$mode $solutionFile

    if ($LastExitCode -ne 0)
    {
        throw "Clean task for $solutionName failed"
    }

    msbuild /p:Configuration=$mode $solutionFile

    if ($LastExitCode -ne 0)
    {
        throw "Build task for $solutionName failed"
    }
}

function RunNUnitTests([Parameter(Mandatory=$false)][String]$solutionName)
{
    $solution = GetSolutionFileInfo $solutionName
    $solutionDirectory = $solution.Directory.FullName

    $testLibraries = get-childitem $solutionDirectory -r | ?{(!$_.PSisContainer) -and ($_.Name -like "*.Tests.dll") -and ($_.DirectoryName -like "*\bin\$mode*")}
    Write-Host "libs $testLibraries"
    $testLibraryRaw = ""
    $testLibraries | %{
        Write-Host "lib $_.Name"
        $testLibraryRaw += "$($_.DirectoryName)\$($_.Name) "
    }
    write-host $testLibraryRaw -foregroundColor cyan

    $testRunnerPath = get-childitem ..\..\packages -r | ?{(!$_.PSisContainer) -and ($_.Name -eq "nunit-console.exe")} 
    $testRunner = $testRunnerPath.FullName

    Invoke-Expression "$testRunner $testLibraryRaw /framework:net-4.5 /nologo /labels"

    CheckTestResults
}

function RunJSUnitTests([Parameter(Mandatory=$false)][String]$solutionName)
{
    $solution = GetSolutionFileInfo $solutionName
    $solutionDirectory = $solution.Directory.FullName

    $testsPath = gci $solutionDirectory -Filter "*.Tests" -Directory
    $testRunnerPath = get-childitem ..\..\packages -r | ?{(!$_.PSisContainer) -and ($_.Name -eq "chutzpah.console.exe")} 

    $result = Invoke-Expression "$($testRunnerPath.FullName) $($testsPath.FullName)"
    $testResult = $result | Select-Object -Last 1

    Write-Host $testResult   

    $versionPattern = " \d+ failed"
    $testResult -match $versionPattern
    $matches[0] -match "\d+"
    if([int]$matches[0] -gt 0)
    {
        throw "ReMi UI unit tests failed: $($matches[0]) failures"
    }
    else 
    {
        Write-Host "All tests passed" -foregroundColor green    
    }
}

function CheckTestResults
{
    if(!(Test-Path .\TestResult.xml))
    {
        throw "Test report was not found"
    }

    [Xml]$testReport = Get-Content .\TestResult.xml

    $failures = (select-xml -xml $testReport -xpath "test-results").Node.failures
    $errors = (select-xml -xml $testReport -xpath "test-results").Node.errors

    if(($failures -ne "0") -or ($errors -ne "0"))
    {
        throw "$failures tests failed"
    }
    else 
    {
        write-host "All tests passed" -foregroundColor green    
    }
}

function GetSolutionFileInfo([Parameter(Mandatory=$false)][String]$solutionName)
{
    return $solution = Gci -r ..\..\ -Filter "$solutionName.sln"
}
