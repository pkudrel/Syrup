using System;
using System.Collections.Generic;
using System.IO;
using Helpers;
using Helpers.MagicVersionService;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    /// Parameters
    [Parameter("Build counter from outside environment")]
    readonly int BuildCounter;

    readonly DateTime BuildDate = DateTime.UtcNow;


    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository] readonly GitRepository GitRepository;
    readonly bool IsAzureDevOps = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_NAME")) == false;

    readonly bool IsTeamCity = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")) == false;

    [Solution] readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath ToolsDir => RootDirectory / "tools";
    AbsolutePath DevDir => RootDirectory / "dev";
    AbsolutePath LibzPath => ToolsDir / "LibZ.Tool" / "tools" / "libz.exe";
    AbsolutePath NugetPath => ToolsDir / "nuget.exe";
    AbsolutePath TmpBuild => TemporaryDirectory / "build";
    AbsolutePath ArtifactsDir => RootDirectory / "artifacts";
    AbsolutePath SourceDir => RootDirectory / "src";

    MagicVersion MagicVersion => MagicVersionFactory.Make(1, 0, 0,
        BuildCounter,
        MagicVersionStrategy.PatchFromCounter,
        BuildDate,
        MachineName);

    ProductInfo ProductInfo => new ProductInfo
    {
        Company = "Deneblab",
        Copyright = $"Deneblab © {DateTime.UtcNow.Year}"
    };

    /// Projects
    Project SyrupProject => Solution.GetProject("Syrup").NotNull();

    Project ScriptExecutorProject => Solution.GetProject("Syrup.ScriptExecutor").NotNull();
    Project SelfProject => Solution.GetProject("Syrup.Self").NotNull();


    List<ProjectDefinition> Projects => new List<ProjectDefinition>
    {
        new ProjectDefinition
        {
            Name = "Syrup",
            Dir = "Syrup",
            Exe = "Syrup.exe",
            DstExe = "Syrup.exe",
            Project = SyrupProject
        },
        new ProjectDefinition
        {
            Name = "SyrupSelf",
            Dir = "Syrup.Self",
            Exe = "Syrup.Self.exe",
            DstExe = "Syrup.Self.exe",
            Project = SelfProject
        },
        new ProjectDefinition
        {
            Name = "Syrup.ScriptExecutor",
            Dir = "Syrup.ScriptExecutor",
            Exe = "Syrup.ScriptExecutor.exe",
            DstExe = "Syrup.ScriptExecutor.exe",
            Project = ScriptExecutorProject
        }
    };




    Target Information => _ => _
        .Executes(() =>
        {
            var b = MagicVersion;
            Logger.Info($"Host: '{Host}'");
            Logger.Info($"Version: '{b.SemVersion}'");
            Logger.Info($"Date: '{b.DateTime:s}Z'");
            Logger.Info($"FullVersion: '{b.InformationalVersion}'");
            Logger.Info($"env:TEAMCITY_VERSION: '{Environment.GetEnvironmentVariable("TEAMCITY_VERSION")}'");
            Logger.Info($"env:Agent.Name: '{Environment.GetEnvironmentVariable("AGENT_NAME")}'");
            Logger.Info(
                $"env:Build.ArtifactStagingDirectory: '{Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY")}'");
        });


    Target ConfigureAzureDevOps => _ => _
        .DependsOn(Information)
        .OnlyWhenStatic(() => IsAzureDevOps)
        .Executes(() =>
        {
            Logger.Info($"Set version to AzureDevOps: {MagicVersion.SemVersion}");
            // https://github.com/microsoft/azure-pipelines-tasks/blob/master/docs/authoring/commands.md
            Logger.Info($"##vso[build.updatebuildnumber]{MagicVersion.SemVersion}");
        });

    Target Configure => _ => _
        .DependsOn(ConfigureAzureDevOps);


    Target CheckTools => _ => _
        .DependsOn(Configure)
        .Executes(() =>
        {
            Downloader.DownloadIfNotExists("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", NugetPath,
                "Nuget");
        });

    Target Clean => _ => _
        .DependsOn(CheckTools)
        .Executes(() =>
        {
            EnsureExistingDirectory(TmpBuild);
            GlobDirectories(TmpBuild, "**/*").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDir);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            using (var process = ProcessTasks.StartProcess(
                NugetPath,
                $"restore  {Solution.Path}",
                SourceDir))
            {
                process.AssertWaitForExit();
                ControlFlow.AssertWarn(process.ExitCode == 0,
                    "Nuget restore report generation process exited with some errors.");
            }
        });


    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>

        {
            foreach (var p in Projects)
            {
                var buildOut = TmpBuild / CommonDir.Build / p.Dir;
                var projectFile = p.Project.Path;
                var projectDir = Path.GetDirectoryName(projectFile);
                EnsureExistingDirectory(buildOut);
                Logger.Info($"Build; Project file: {projectFile}");
                Logger.Info($"Build; Project dir: {projectDir}");
                Logger.Info($"Build; Out dir: {buildOut}");
                Logger.Info($"Build; Target: {Configuration}");
                Logger.Info($"Build; Target: {GitRepository.Branch}");

                //AssemblyTools.Patch(projectDir, MagicVersion, p, ProductInfo);

                MSBuild(s => s
                    .SetProjectFile(projectFile)
                    .SetOutDir(buildOut)
                    .SetVerbosity(MSBuildVerbosity.Quiet)
                    .SetConfiguration(Configuration)
                    .SetTargetPlatform(MSBuildTargetPlatform.x64)
                    .SetMaxCpuCount(Environment.ProcessorCount)
                    .SetNodeReuse(IsLocalBuild));
            }
        });
    Target Marge => _ => _
        .DependsOn(Compile)
        .Executes(() =>

        {
            foreach (var p in Projects)
            {
                var buildOut = TmpBuild / CommonDir.Build / p.Dir;
                var margeOut = TmpBuild / CommonDir.Merge / p.Dir;

                EnsureExistingDirectory(margeOut);
                CopyDirectoryRecursively(buildOut, margeOut, DirectoryExistsPolicy.Merge);

                using (var process = ProcessTasks.StartProcess(
                    LibzPath,
                    $"inject-dll --assembly {p.Exe} --include *.dll --move",
                    margeOut))
                {
                    process.AssertWaitForExit();
                    ControlFlow.AssertWarn(process.ExitCode == 0,
                        "Libz report generation process exited with some errors.");
                }
            }
        });


    public static int Main() => Execute<Build>(x => x.Marge);
}