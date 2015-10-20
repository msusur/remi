Get-Module | % { if($_.Name -eq "BuildRemi") { Remove-Module BuildReMi}}
Import-Module .\BuildReMi.psm1 -disablenamechecking

$solutionName = "ReMi.Web"

BuildSolution $solutionName
RunJSUnitTests $solutionName