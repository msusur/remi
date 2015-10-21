function CommitPackage([Parameter(Mandatory=$true)][String]$packageName,
    [Parameter(Mandatory=$true)][String]$version,
    [Parameter(Mandatory=$false)][String]$push=$false)
{
    Write-Host 'CommitPackage - started'

    git commit --all --message "New $packageName-$version package"
    git tag --force "Package$packageName"
    git tag "Package$packageName-$version"

    Write-Host 'CommitPackage - finished'

    if ($push -eq $true) { GitPush }

	if (!$?){
		throw "Failed to add t"
	}
}


function GitPush
{
    Write-Host 'GitPush - started'

    git push

    Write-Host 'GitPush - finished'

	if (!$?){
		throw "Failed to add t"
	}
}
