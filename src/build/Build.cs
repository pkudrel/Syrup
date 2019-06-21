using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Helpers;
using Helpers.Azure;
using Helpers.MagicVersionService;
using Helpers.Syrup;
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
            Name = SyrupProject.Name,
            Dir = SyrupProject.Name,
            Exe = "Syrup.exe",
            DstExe = "Syrup.exe",
            Project = SyrupProject
        },
        new ProjectDefinition
        {
            Name = SelfProject.Name,
            Dir = SelfProject.Name,
            Exe = "Syrup.Self.exe",
            DstExe = "Syrup.Self.exe",
            Project = SelfProject
        },
        new ProjectDefinition
        {
            Name = ScriptExecutorProject.Name,
            Dir = ScriptExecutorProject.Name,
            Exe = "Syrup.ScriptExecutor.exe",
            DstExe = "Syrup.ScriptExecutor.exe",
            Project = ScriptExecutorProject
        }
    };


    Target Information => _ => _
        .Executes(() =>
        {
            var b = MagicVersion;
            Logger.Normal($"Host: '{Host}'");
            Logger.Normal($"Version: '{b.SemVersion}'");
            Logger.Normal($"Date: '{b.DateTime:s}Z'");
            Logger.Normal($"FullVersion: '{b.InformationalVersion}'");
            Logger.Normal($"env:TEAMCITY_VERSION: '{Environment.GetEnvironmentVariable("TEAMCITY_VERSION")}'");
            Logger.Normal($"env:Agent.Name: '{Environment.GetEnvironmentVariable("AGENT_NAME")}'");
            Logger.Normal(
                $"env:Build.ArtifactStagingDirectory: '{Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY")}'");
        });


    Target ConfigureAzureDevOps => _ => _
        .DependsOn(Information)
        .OnlyWhenStatic(() => IsAzureDevOps)
        .Executes(() =>
        {
            Logger.Normal($"Set version to AzureDevOps: {MagicVersion.SemVersion}");
            // https://github.com/microsoft/azure-pipelines-tasks/blob/master/docs/authoring/commands.md
            Logger.Normal($"##vso[build.updatebuildnumber]{MagicVersion.SemVersion}");
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

    Target MakeComponents => _ => _
        .DependsOn(MakeSyrupSelf, MakeScriptExecutor, CopyComponents);


    Target MakeSyrupSelf => _ => _
        .DependsOn(Restore)
        .Executes(() =>

        {
            var project = Projects.FirstOrDefault(x => x.Project == SelfProject);
            BuildFn(project);
            MargeFn(project);
        });


    Target MakeScriptExecutor => _ => _
        .DependsOn(Restore)
        .Executes(() =>

        {
            var project = Projects.FirstOrDefault(x => x.Project == ScriptExecutorProject);
            BuildFn(project);
            MargeFn(project);
        });

    Target MakeSyrup => _ => _
        .DependsOn(MakeComponents)
        .Executes(() =>

        {
            var project = Projects.FirstOrDefault(x => x.Project == SyrupProject);
            BuildFn(project);
            MargeFn(project);
        });


    Target CopyComponents => _ => _
        .DependsOn(MakeSyrupSelf, MakeScriptExecutor)
        .Executes(() =>

        {
            var embedDir = SyrupProject.Directory / "Embed";
            var selfProjectExe = TmpBuild / CommonDir.Merge / SelfProject.Name / $"{SelfProject.Name}.exe";
            var scriptExecutorProjectExe = TmpBuild / CommonDir.Merge / ScriptExecutorProject.Name /
                                           $"{ScriptExecutorProject.Name}.exe";
            ;
            CopyFile(selfProjectExe, embedDir / "syrup-self.bin", FileExistsPolicy.Overwrite);
            CopyFile(scriptExecutorProjectExe, embedDir / "syrup-executor.bin", FileExistsPolicy.Overwrite);
        });


    Target Nuget => _ => _
        .DependsOn(MakeSyrup)
        .Executes(() =>

        {
            var p = Projects.FirstOrDefault(x => x.Project == SyrupProject);

            var tmpMerge = TmpBuild / CommonDir.Merge / p.Dir;
            var tmpNuget = TmpBuild / CommonDir.Nuget / p.Dir;
            var tmpMain = tmpNuget / "main" / p.Dir;
            var tmpOthers = tmpNuget / "others";
            var srcBuild = SourceDir / CommonDir.Build;
            var tmpReady = TmpBuild / CommonDir.Ready;

            EnsureExistingDirectory(tmpNuget);
            EnsureExistingDirectory(tmpReady);


            // main dir
            EnsureExistingDirectory(tmpMain);
            CopyFile(tmpMerge / "Syrup.exe", tmpMain / "Syrup.exe");


            // nuget definition
            var srcNugetFile = srcBuild / "nuget" / "nuget.nuspec";
            var dstNugetFile = tmpNuget / "Syrup.nuspec";
            //CopyFile(srcBuild / "nuget" / "nuget.nuspec", dstNugetFile);

            var text = File.ReadAllText(srcNugetFile);
            var r = text.Replace("{Version}", MagicVersion.NugetVersion);
            File.WriteAllText(dstNugetFile, r, Encoding.UTF8);


            using (var process = ProcessTasks.StartProcess(
                NugetPath,
                $"pack {dstNugetFile} -OutputDirectory {tmpReady} -NoPackageAnalysis",
                tmpNuget))
            {
                process.AssertWaitForExit();
                ControlFlow.AssertWarn(process.ExitCode == 0,
                    "Nuget report generation process exited with some errors.");
            }

            var nugetFiles = GlobFiles(tmpReady, "*.nupkg");

            foreach (var file in nugetFiles) SyrupTools.MakeSyrupFile(file, MagicVersion, p);
        });

    Target PublishAzureDevOps => _ => _
        .DependsOn(Nuget)
        .OnlyWhenStatic(() => IsAzureDevOps)
        .Executes(() =>
        {
            var tmpReady = TmpBuild / CommonDir.Ready;
            var serverPublishArtifact = Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY");
            CopyDirectoryRecursively(tmpReady, serverPublishArtifact, DirectoryExistsPolicy.Merge);
        });

    Target PublishAzureDevOpsStorage => _ => _
        .DependsOn(PublishAzureDevOps)
        .OnlyWhenStatic(() => IsAzureDevOps)
        .Executes(async () =>
        {
            void LogFiles(string title, List<SyrupInfo> filesToShow)
            {
                Logger.Info($"{title}: {filesToShow.Count}");
                foreach (var l in filesToShow) Logger.Info($"Name: {l.Name}; Date: {l.ReleaseDate}");
            }

            var storageConnectionString = Environment.GetEnvironmentVariable("azureStorageConnectionString");
            Logger.Info($"Build; storageConnectionString: {storageConnectionString}");
            var serverPublishArtifact = Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY");
            var files = Directory.GetFiles(serverPublishArtifact).ToList();
            var client = AzureSyrupTools.Create(storageConnectionString, "syrup");
            await client.UploadFiles(files);
            var list = await client.GetSyrupFiles();
            var fileToRemove = list.OrderByDescending(x => x.ReleaseDate).Skip(15).ToList();
            LogFiles("Files to remove", fileToRemove);
            await client.RemoveSyrupFiles(fileToRemove);
            var newList = await client.GetSyrupFiles();
            await client.CreateSyrupFilesList(newList);
            LogFiles("Files in container", newList);
        });


    Target Publish => _ => _
        .DependsOn(Nuget, PublishAzureDevOps, PublishAzureDevOpsStorage);

    void BuildFn(ProjectDefinition p)
    {
        var buildOut = TmpBuild / CommonDir.Build / p.Dir;
        var projectFile = p.Project.Path;
        var projectDir = Path.GetDirectoryName(projectFile);
        EnsureExistingDirectory(buildOut);
        Logger.Normal($"Build; Project file: {projectFile}");
        Logger.Normal($"Build; Project dir: {projectDir}");
        Logger.Normal($"Build; Out dir: {buildOut}");
        Logger.Normal($"Build; Target: {Configuration}");
        Logger.Normal($"Build; Target: {GitRepository.Branch}");

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

    void MargeFn(ProjectDefinition p)
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

    public static int Main() => Execute<Build>(x => x.Publish);
}