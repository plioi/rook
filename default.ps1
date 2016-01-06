Framework '4.0'

properties {
    $project = "Rook"
    $birthYear = 2007
    $maintainers = "Patrick Lioi"
    $description = "A .NET programming language"

    $configuration = 'Release'
    $src = resolve-path '.\src'
    $version = [IO.File]::ReadAllText('.\VERSION.txt')
}

task default -depends Test

task Test -depends Compile {
    $fixieRunner = join-path $src "packages\Fixie.0.0.1.218\lib\net45\Fixie.Console.exe"
    exec { & $fixieRunner $src\$project.Test\bin\$configuration\$project.Test.dll $src\$project.IntegrationTest\bin\$configuration\$project.IntegrationTest.dll }
}

task Compile -depends CommonAssemblyInfo {
  exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration $src\$project.sln }
  exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration $src\$project.sln }
}

task CommonAssemblyInfo {
    $assemblyVersion = $version
    if ($assemblyVersion.Contains("-")) {
        $assemblyVersion = $assemblyVersion.Substring(0, $assemblyVersion.IndexOf("-"))
    }

    $date = Get-Date
    $year = $date.Year
    $copyrightSpan = if ($year -eq $birthYear) { $year } else { "$birthYear-$year" }
    $copyright = "Copyright (c) $copyrightSpan $maintainers"

"using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct(""$project"")]
[assembly: AssemblyVersion(""$assemblyVersion"")]
[assembly: AssemblyFileVersion(""$assemblyVersion"")]
[assembly: AssemblyInformationalVersion(""$version"")]
[assembly: AssemblyCopyright(""$copyright"")]
[assembly: AssemblyCompany(""$maintainers"")]
[assembly: AssemblyDescription(""$description"")]
[assembly: AssemblyConfiguration(""$configuration"")]" | out-file "$src\CommonAssemblyInfo.cs" -encoding "ASCII"
}
