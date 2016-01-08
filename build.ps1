Framework '4.5.2'

properties {
    $birthYear = 2007
    $maintainers = "Patrick Lioi"
    $description = "A .NET programming language"

    $configuration = 'Debug'
    $src = resolve-path '.\src'
    $projects = @(gci $src -rec -filter *.csproj)
    $version = [IO.File]::ReadAllText('.\VERSION.txt')
}

task default -depends Test

task Test -depends Compile {
    $testRunners = @(gci $src\packages -rec -filter Fixie.Console.exe)

    if ($testRunners.Length -ne 1)
    {
        throw "Expected to find 1 Fixie.Console.exe, but found $($testRunners.Length)."
    }

    $testRunner = $testRunners[0].FullName

    foreach ($project in $projects)
    {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)

        if ($projectName.EndsWith("Test"))
        {
            $testAssembly = "$($project.Directory)\bin\$configuration\$projectName.dll"
            exec { & $testRunner $testAssembly }
        }
    }
}

task Compile -depends AssemblyInfo, License {
  exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration $src\Rook.sln }
  exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration $src\Rook.sln }
}

task AssemblyInfo {
    $assemblyVersion = $version
    if ($assemblyVersion.Contains("-")) {
        $assemblyVersion = $assemblyVersion.Substring(0, $assemblyVersion.IndexOf("-"))
    }

    $copyright = get-copyright

    foreach ($project in $projects) {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)

        if ($projectName -eq "Build") {
            continue;
        }

        regenerate-file "$($project.DirectoryName)\Properties\AssemblyInfo.cs" @"
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct("Rook")]
[assembly: AssemblyTitle("$projectName")]
[assembly: AssemblyVersion("$assemblyVersion")]
[assembly: AssemblyFileVersion("$assemblyVersion")]
[assembly: AssemblyInformationalVersion("$version")]
[assembly: AssemblyCopyright("$copyright")]
[assembly: AssemblyCompany("$maintainers")]
[assembly: AssemblyConfiguration("$configuration")]
"@
    }
}

task License {
    $copyright = get-copyright

    regenerate-file "LICENSE.txt" @"
The MIT License (MIT)
$copyright

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
"@
}

function get-copyright {
    $date = Get-Date
    $year = $date.Year
    $copyrightSpan = if ($year -eq $birthYear) { $year } else { "$birthYear-$year" }
    return "Copyright � $copyrightSpan $maintainers"
}

function regenerate-file($path, $newContent) {
    $oldContent = [IO.File]::ReadAllText($path)

    if ($newContent -ne $oldContent) {
        $relativePath = Resolve-Path -Relative $path
        write-host "Generating $relativePath"
        [System.IO.File]::WriteAllText($path, $newContent, [System.Text.Encoding]::UTF8)
    }
}