using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.Pack);

    [Solution] readonly Solution Solution;

    Project NullableProject => Solution.GetProject("Nullable");

    IEnumerable<FileInfo> AttributeFiles => Directory
        .GetFiles(NullableProject.Directory, "*Attribute.cs", SearchOption.AllDirectories)
        .Select(path => new FileInfo(path));

    FileInfo NuspecFile => new FileInfo(RootDirectory / "src" / "Nullable.nuspec");

    DirectoryInfo ArtifactsDirectory => new DirectoryInfo(RootDirectory / "artifacts");

    DirectoryInfo OutDirectory => new DirectoryInfo(TemporaryDirectory / "out");

    DirectoryInfo ExcludeFromCodeCoverageOutDirectory => new DirectoryInfo(TemporaryDirectory / "out" / "ExcludeFromCodeCoverage");

    DirectoryInfo NoExcludeFromCodeCoverageOutDirectory => new DirectoryInfo(TemporaryDirectory / "out" / "NoExcludeFromCodeCoverage");

    Target Clean => _ => _
        .Executes(() =>
        {
            DeleteDirectory(ArtifactsDirectory.FullName);
            DeleteDirectory(ExcludeFromCodeCoverageOutDirectory.FullName);
            DeleteDirectory(NoExcludeFromCodeCoverageOutDirectory.FullName);
        });

    Target Compile => _ => _
        .Executes(() =>
        {
            // We build once to verify that the sources don't contain errors.
            // Has no function apart from that since the build output is not used.
            DotNetBuild(s => s
                .SetProjectFile(NullableProject.Path)
                .SetIgnoreFailedSources(true));
        });

    Target CreateOutFiles => _ => _
        .DependsOn(Clean)
        .DependsOn(Compile)
        .Executes(() =>
        {
            // Nullable uses the [ExcludeFromCodeCoverage] attribute.
            // This is not available in certain target frameworks (e.g. .NET 2.0 and .NET Standard 1.0).
            // For this reason, we remove that attribute from the source code.
            // This results in two different file versions. These are temporarily stored in the output directories
            // and then used by the nuspec accordingly.
            ExcludeFromCodeCoverageOutDirectory.Create();
            NoExcludeFromCodeCoverageOutDirectory.Create();

            foreach (var attributeFile in AttributeFiles)
            {
                attributeFile.CopyTo(Path.Combine(ExcludeFromCodeCoverageOutDirectory.FullName, attributeFile.Name));
                attributeFile.CopyTo(Path.Combine(NoExcludeFromCodeCoverageOutDirectory.FullName, attributeFile.Name));
            }

            foreach (var noExcludeFromCodeCoverageFile in NoExcludeFromCodeCoverageOutDirectory.GetFiles())
            {
                var content = File.ReadAllText(noExcludeFromCodeCoverageFile.FullName);
                content = content.Replace("ExcludeFromCodeCoverage, ", "");
                File.WriteAllText(noExcludeFromCodeCoverageFile.FullName, content);
            }
        });

    Target Pack => _ => _
        .DependsOn(CreateOutFiles)
        .Executes(() =>
        {
            NuGetPack(s => s
                .SetTargetPath(NuspecFile.FullName)
                .SetBasePath(OutDirectory.FullName)
                .SetOutputDirectory(ArtifactsDirectory.FullName));
        });
}
