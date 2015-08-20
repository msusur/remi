Push-Location ..\

foreach ($ext in @("*.cs", "*.js", "*.html", "*.csproject", "*.sln", "*.css"))  {
    (dir -Recurse -Filter $ext) | foreach { 
        $file = gc $_.FullName
        $file | sc $_.FullName
        }

}

Push-Location BuildScripts
