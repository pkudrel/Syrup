using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using NLog;
using Optional;
using Optional.Unsafe;
using Syrup.Common;
using Syrup.Common.Io;
using Syrup.Core.Local.Models;
using Syrup.Core.Server.Models;
using Syrup.Core._Infrastructure.Global;

namespace Syrup.Core.Local.Services
{
    public interface IFileManagerService
    {
        List<LocalReleaseInfo> ScanLocalFileSystem();
        Option<LocalReleaseInfo> TryGetActiveApp();
        List<LocalReleaseInfo> ScanNugetsDirPath();
        List<LocalReleaseInfo> ScanAppsDirPath();
        void PutReleaseToNugetDir(LocalReleaseInfo release, string path);
        void ExtractNugetToApps(LocalReleaseInfo release);
        void CopyToActiveDir(LocalReleaseInfo release);
        List<LocalReleaseInfo> DeleteOldRelease(List<LocalReleaseInfo> res0, int maxNumberLocalRealses);
    }

    public class FileManagerService : IFileManagerService
    {
        private readonly string _appsDirPath;
        private readonly Registry _registry;
        private readonly string _syrupNugetsDirPath;
        private string _syrupTmpDirPath;
        private string _syrupWorkDirPath;
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        public FileManagerService(Registry registry)
        {
            _registry = registry;
            _syrupWorkDirPath = registry.SyrupWorkDirPath;
            _syrupNugetsDirPath = registry.SyrupNugetsDirPath;
            _appsDirPath = registry.AppsDirPath;
            _syrupTmpDirPath = registry.SyrupTmpDirPath;
        }

        public List<LocalReleaseInfo> ScanLocalFileSystem()
        {
            var nugets = ScanNugetsDirPath();
            var apps = ScanAppsDirPath();
            var activeOption = TryGetActiveApp();
            var appsDic = apps.ToDictionary(x => x.Name, y => y);

            foreach (var localReleaseInfo in nugets)
            {
                LocalReleaseInfo val;
                if (appsDic.TryGetValue(localReleaseInfo.Name, out val))
                    val.IsLocalNuget = localReleaseInfo.IsLocalNuget;
                else
                    appsDic.Add(localReleaseInfo.Name, localReleaseInfo);
            }
            if (activeOption.HasValue)
            {
                LocalReleaseInfo val;
                var localReleaseInfo = activeOption.ValueOrFailure();
                if (appsDic.TryGetValue(localReleaseInfo.Name, out val))
                    val.IsActive = localReleaseInfo.IsActive;
                else
                    appsDic.Add(localReleaseInfo.Name, localReleaseInfo);
            }
            return appsDic.Select(x => x.Value).ToList();
        }


        public Option<LocalReleaseInfo> TryGetActiveApp()
        {
            var dir = new DirectoryInfo(_appsDirPath);
            var file = dir.GetFiles(Consts.SYRUP_ACTIVE_APP_FILE, SearchOption.AllDirectories).FirstOrDefault();
            var parentDir = file?.Directory;
            if (parentDir != null)
            {
                var syrupFile = GetSyrupPath(parentDir.FullName).FirstOrDefault();
                if (syrupFile != null)
                {
                    var syrup = JsonConvert.DeserializeObject<SyrupFileInfo>(File.ReadAllText(syrupFile.FullName));
                    return Option.Some(new LocalReleaseInfo
                    {
                        App = syrup.App,
                        Name = syrup.Name,
                        File = syrup.File,
                        Sha = syrup.Sha,
                        SemVer = syrup.SemVer,
                        RelaseDate = syrup.RelaseDate,
                        IsActive = true
                    });
                }
            }
            return Option.None<LocalReleaseInfo>();
        }

        public List<LocalReleaseInfo> ScanNugetsDirPath()
        {
            var list = new List<LocalReleaseInfo>();

            var files = GetSyrupPath(_syrupNugetsDirPath);
            foreach (var fileInfo in files)
            {
                var syrup = JsonConvert.DeserializeObject<SyrupFileInfo>(File.ReadAllText(fileInfo.FullName));
                var item = new LocalReleaseInfo
                {
                    App = syrup.App,
                    Name = syrup.Name,
                    File = syrup.File,
                    Sha = syrup.Sha,
                    SemVer = syrup.SemVer,
                    RelaseDate = syrup.RelaseDate,
                    IsLocalNuget = true
                };
                list.Add(item);
            }
            return list;
        }

        public List<LocalReleaseInfo> ScanAppsDirPath()
        {
            var list = new List<LocalReleaseInfo>();
            var files = GetSyrupPath(_appsDirPath);
            foreach (var fileInfo in files)
            {
                var syrup = JsonConvert.DeserializeObject<SyrupFileInfo>(File.ReadAllText(fileInfo.FullName));
                var parentDir = fileInfo.Directory;
                if ((parentDir == null) ||
                    parentDir.GetFiles(Consts.SYRUP_ACTIVE_APP_FILE, SearchOption.TopDirectoryOnly).Any()) continue;
                var item = new LocalReleaseInfo
                {
                    App = syrup.App,
                    Name = syrup.Name,
                    File = syrup.File,
                    Sha = syrup.Sha,
                    SemVer = syrup.SemVer,
                    RelaseDate = syrup.RelaseDate,
                    IsExtracted = true
                };
                list.Add(item);
            }
            return list;
        }

        public void PutReleaseToNugetDir(LocalReleaseInfo release, string path)
        {
            var filePathInNugets = Path.Combine(_syrupNugetsDirPath, release.Name, release.File);
            var dirPathInNugets = Path.GetDirectoryName(filePathInNugets);
            Misc.CreateDirIfNotExist(dirPathInNugets);
            Misc.RemoveFilesInDir(dirPathInNugets);
            File.Move(path, filePathInNugets);
            GenerateSyrupFile(release, filePathInNugets);
            release.IsLocalNuget = true;
        }

        public void ExtractNugetToApps(LocalReleaseInfo release)
        {
            var pathInNuget = Path.Combine(_syrupNugetsDirPath, release.Name, release.File);
            var syrupInNuget = Path.Combine(_syrupNugetsDirPath, release.Name,
                release.File + Consts.SYRUP_FILE_EXTENSION);

            if (File.Exists(pathInNuget))
            {
                var dirInApps = Path.Combine(_appsDirPath, release.Name);
                Misc.CreateDirIfNotExist(dirInApps);
                Misc.RemoveFilesAndDirsInDir(dirInApps);
                ZipFile.ExtractToDirectory(pathInNuget, dirInApps);

                if (File.Exists(syrupInNuget))
                {
                    var filePath = Path.Combine(dirInApps, release.File + Consts.SYRUP_FILE_EXTENSION);

                    File.Copy(syrupInNuget, filePath);
                    release.IsExtracted = true;
                    CleanNugetRemains(dirInApps);
                }
            }
        }

        public void CopyToActiveDir(LocalReleaseInfo release)
        {
            var versionAppDirPath = Path.Combine(_appsDirPath, release.Name);
            var currentAppDirPath = Path.Combine(_appsDirPath, release.App);
            Misc.RemoveFilesAndDirsInDir(currentAppDirPath);

            var di = new DirectoryInfo(versionAppDirPath);
            foreach (var d in di.GetDirectories())
            {
                var dst = Path.Combine(currentAppDirPath, d.Name);
                Misc.CopyDirectory(d.FullName, dst);
            }

            var syrupFile = release.File + Consts.SYRUP_FILE_EXTENSION;
            var srcSyrupFile = Path.Combine(_appsDirPath, release.Name, syrupFile);

            if (File.Exists(srcSyrupFile))
            {
                var dstSyrupFile = Path.Combine(currentAppDirPath, syrupFile);
                File.Copy(srcSyrupFile, dstSyrupFile);
            }
            var activeFile = Path.Combine(currentAppDirPath, "syrup-active-config.json");
            File.WriteAllText(activeFile, "{}");
        }

        public List<LocalReleaseInfo> DeleteOldRelease(List<LocalReleaseInfo> res0, int maxNumberLocalRealses)
        {
            var files = res0.OrderByDescending(x => x.RelaseDate);
            var toTake = files.Take(maxNumberLocalRealses);
            var toRemove = files.Skip(maxNumberLocalRealses);
            foreach (var localReleaseInfo in toRemove)
            {
                var pathNuget = Path.Combine(_syrupNugetsDirPath, localReleaseInfo.Name);
                if (Directory.Exists(pathNuget))
                {
                    _log.Debug($"Old release in nuget dir: {pathNuget}");
                    Misc.RemovedDir(pathNuget);
                }
                var pathApp = Path.Combine(_appsDirPath, localReleaseInfo.Name);
                if (Directory.Exists(pathApp))
                {
                    _log.Debug($"Old release in apps dir: {pathApp}");
                    Misc.RemovedDir(pathApp);
                }
            }

            return toTake.ToList();
        }

        private void CleanNugetRemains(string dirInApps)
        {
            var dir = new DirectoryInfo(dirInApps);
            Misc.RemovedDir(Path.Combine(dirInApps, "_rels"));
            Misc.RemovedDir(Path.Combine(dirInApps, "package"));
            Misc.RemoveFile(Path.Combine(dirInApps, "[Content_Types].xml"));
            var nugetSpec = dir.GetFiles("*.nuspec", SearchOption.TopDirectoryOnly);
            foreach (var fileInfo in nugetSpec)
                Misc.RemoveFile(fileInfo.FullName);
        }

        private void GenerateSyrupFile(LocalReleaseInfo release, string filePath)
        {
            var sha = GetSha1Hash(filePath);
            var file = Path.GetFileName(filePath);
            var syrupFilePath = filePath + Consts.SYRUP_FILE_EXTENSION;

            var res = new SyrupFileInfo
            {
                App = release.App,
                Name = release.Name,
                File = file,
                RelaseDate = release.RelaseDate,
                SemVer = release.SemVer,
                Sha = sha
            };
            var json = JsonConvert.SerializeObject(res, Formatting.Indented);
            File.WriteAllText(syrupFilePath, json);
        }

        public string GetSha1Hash(string filePath)
        {
            using (var fs = File.OpenRead(filePath))
            {
                SHA1 sha = new SHA1Managed();
                return BitConverter.ToString(sha.ComputeHash(fs)).Replace("-", string.Empty);
                ;
            }
        }

        private FileInfo[] GetSyrupPath(string path)
        {
            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles($"*{Consts.SYRUP_FILE_EXTENSION}", SearchOption.AllDirectories);
            return files;
        }
    }
}