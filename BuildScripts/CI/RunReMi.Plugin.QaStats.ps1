param([Boolean]$skipTest=$false)
Get-Module | % { if($_.Name -eq "BuildRemi") { Remove-Module BuildReMi}}
Import-Module .\BuildReMi.psm1 -disablenamechecking

$solutionName = "ReMi.Plugin.QaStats"

BuildSolution $solutionName
RunNUnitTests $solutionName