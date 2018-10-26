using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Syrup.Self.Common;
using Syrup.Self.Parts.FileDownloader;
using Syrup.Self.Parts.Globals;
using Syrup.Self.Parts.Helpers;
using Syrup.Self.Parts.Log;
using Syrup.Self.Parts.Sem;
using Syrup.Self.Parts.ServerRealses;
using Version = Syrup.Self.Common.Version;

namespace Syrup.Self.Parts.SelfUpdater
{
    public class SelfUpdaterService
    {
        private static readonly Logger _log = Logger.Instance;
        private readonly Global _global;
        private readonly INotifier _notifier;
        private readonly SyrupDowloader _syrupDowloader;

        public SelfUpdaterService(Global global, SyrupDowloader syrupDowloader, INotifier notifier)
        {
            _global = global;
            _syrupDowloader = syrupDowloader;
            _notifier = notifier;
        }


        public async Task MakeUpdate()
        {
            _notifier.AddToLog("Begin...");
            var releaseInfo = await _syrupDowloader.GetNewestSyrupFileInfo(_global.Config.SyrupReleaseInfoUrl);
            if (!IsUpdateNeeded(_global.Registry.CurrentAssemblyVersion, releaseInfo))
            {
                await MakeExit();
                return;
            }

            var tmpDir = GetTmpDir();
            var zipFile = await DownloadZipFile(tmpDir, releaseInfo);
            var newSyrupFile = ExtractZipFile(zipFile);
            var oldSyrupBeckupFile = await MakeBeckupOldSyrup(_global.Registry.SyrupAppPath);
            await SetNewVersion(newSyrupFile, _global.Registry.SyrupAppPath);
            await Clean(tmpDir, oldSyrupBeckupFile);
            await MakeExit();
        }

        private async Task Clean(string tmpDir, string oldSyrupBeckupFile)
        {
            _notifier.AddToLog("Clean");
            _log.Debug("Clean");
            Directory.Delete(tmpDir, true);
            while (Directory.Exists(tmpDir)) await Task.Delay(500);
            Io.DeleteFileAndWait(oldSyrupBeckupFile);
            while (File.Exists(oldSyrupBeckupFile)) await Task.Delay(500);
        }

        private async Task<string> DownloadZipFile(string tmpDir, ReleaseInfo releaseInfo)
        {
            _notifier.AddToLog($"Download file: {releaseInfo.File}");
            _log.Debug($"Download file: {releaseInfo.File}");
            var newSyrupTmpZipFile = Path.Combine(tmpDir, releaseInfo.File + ".tmp");
            var newSyrupZipFile = Path.Combine(tmpDir, releaseInfo.File);
            _log.Debug($"newSyrupTmpZipFile: {newSyrupTmpZipFile}; newSyrupZipFile: {newSyrupZipFile}");
            await _syrupDowloader.DownloadFile(releaseInfo.FileUrl, newSyrupTmpZipFile);
            while (!File.Exists(newSyrupTmpZipFile)) await Task.Delay(500);
            File.Move(newSyrupTmpZipFile, newSyrupZipFile);
            return newSyrupZipFile;
        }


        private async Task<string> MakeBeckupOldSyrup(string pathToOldSyrup)
        {
            _notifier.AddToLog($"Make backup old syrup");
            _log.Debug($"Make backup old syrup");
            var backupFile = pathToOldSyrup + ".old";
            _log.Debug($"pathToOldSyrup: {pathToOldSyrup}; backupFile: {backupFile}");
            await Io.MoveFile(pathToOldSyrup, backupFile);
            return backupFile;
        }

        private async Task SetNewVersion(string tmpPathToNew, string validPathToSyrup)
        {
            _notifier.AddToLog($"Copy new version");
            _log.Debug($"Copy new version");
            _log.Debug($"tmpPathToNew: {tmpPathToNew}; validPathToSyru: {validPathToSyrup}");
            File.Move(tmpPathToNew, validPathToSyrup);
            while (!File.Exists(validPathToSyrup)) await Task.Delay(500);
        }

        private string GetTmpDir()
        {
            var guid = Guid.NewGuid().ToString();
            var tmpDir = Path.Combine(_global.Registry.SyrupTmpDirPath, guid);
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir);
            Directory.CreateDirectory(tmpDir);
            return tmpDir;
        }

        private string ExtractZipFile(string zipfile)
        {
            _notifier.AddToLog($"Extract zip file");
            _log.Debug($"Extract zip file");
            var di = new FileInfo(zipfile);
            var dir = di.Directory;
            if (dir != null) ZipFile.ExtractToDirectory(zipfile, dir.FullName);
            if (dir == null) throw new FileNotFoundException($"Cannot find directory for: {zipfile}");
            _log.Debug($"zipfile: {zipfile}; dir: {dir.FullName}");
            var file = dir.GetFiles("*.exe").FirstOrDefault();
            if (file == null) throw new FileNotFoundException($"Cannot find exe file in: {zipfile}");
            return file.FullName;
        }

        private async Task MakeExit()
        {
            var counter = 5;
            _notifier.AddToLog($"Exiting...");

            while (counter >= 1)
            {
                await Task.Delay(1000);
                _notifier.AddToLog($"{counter}");
                counter--;
            }
            _notifier.AddToLog($"Bye...");
            Process.Start(_global.Registry.SyrupAppPath);
            _notifier.CloseMe();
        }


        private bool IsUpdateNeeded(Version currentVersion, ReleaseInfo releaseInfo)
        {
            _log.Debug($"Is Update Needed");
            var currentSem = SemVersion.Parse(currentVersion.SemVer);
            var newestSem = SemVersion.Parse(releaseInfo.SemVer);
            _log.Debug($"Current syrup version: {currentSem} - newest version on server: {newestSem}");
            _notifier.AddToLog($"Current syrup version: {currentSem} - newest version on server: {newestSem}");
            if (currentSem < newestSem)
            {
                _notifier.AddToLog($"Update is needed {currentSem} ===> {newestSem}");
                _log.Debug($"Update is needed {currentSem} ===> {newestSem}");
                return true;
            }
            if (currentSem == newestSem)
                _notifier.AddToLog($"Update not needed");
            _log.Debug($"Update not needed");
            return false;
        }
    }
}