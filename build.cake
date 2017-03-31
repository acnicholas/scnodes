using Cake.Common.Diagnostics;
using System.IO;

var target = Argument("target", "Default");
var solutionFile = GetFiles("SCDynamoNodes.sln").First();
var buildDir = Directory(@"bin/Debug");

public MSBuildSettings GetBuildSettings(string config)
{
    return new MSBuildSettings()
			.SetConfiguration(config)
			.WithTarget("Clean,Build")
            .WithProperty("Platform","x64")
            .SetVerbosity(Verbosity.Normal);
}

public bool APIAvailable(string revitVersion)
{
    return FileExists(@"C:\Program Files\Autodesk\Revit " + revitVersion + @"\RevitAPI.dll");
}

Task("Clean").Does(() => CleanDirectory(buildDir));

Task("Restore-NuGet-Packages").Does(() => NuGetRestore(solutionFile));

Task("Revit2016")
    .IsDependentOn("Restore-NuGet-Packages")
    .WithCriteria(APIAvailable("2016"))
    .Does(() => MSBuild(solutionFile, GetBuildSettings("Debug")));

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Revit2016");

RunTarget(target);
