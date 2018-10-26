using System;
using System.IO;
using Syrup.Common;
using Syrup.Common.App;
using Syrup.Common.Bootstrap;
using Syrup.Common.Version;
using Version = Syrup.Common.Version.Version;

namespace Syrup.Core._Infrastructure.Global
{
    public class Registry
    {
        public Registry(string syrupDirPath, string syrupConfigDirectoryPath, Config config)
        {
            Configure(syrupDirPath, syrupConfigDirectoryPath, config);
        }


        public Registry(AppEnvironment env, Config config)
        {
            var mainDir = env.IsDeveloperMode ? env.RootDir : env.ExeFileDir;
            Configure(mainDir, mainDir, config);
        }

        public string SyrupDirPath { get; private set; }
        public string SyrupConfigDirectoryPath { get; private set; }
        public string SyrupNugetsDirPath { get; private set; }
        public string AppsDirPath { get; private set; }
        public string SyrupWorkDirPath { get; private set; }
        public string SyrupSelfDirPath { get; private set; }
        public string SyrupSelfFilePath { get; private set; }


        public string SyrupExecutorDirPath { get; private set; }
        public string SyrupExecutorFilePath { get; private set; }

        public string SyrupTmpDirPath { get; private set; }
        public string AppsLogsDirPath { get; private set; }
        public string AppsConfigsDirPath { get; private set; }
        public string AppsConfigsForMachineDirPath { get; private set; }
        public string Key { get; private set; }
        public string ReleaseInfoUrl { get; private set; }
        public string SyrupReleaseInfoUrl { get; private set; }
        public Version Version { get; private set; }


        private void Configure(string syrupDirPath, string syrupConfigDirectoryPath, Config config)
        {
            var configWorkDir = string.IsNullOrEmpty(config.WorkDirPath) ? syrupDirPath : config.WorkDirPath;
            SyrupDirPath = syrupDirPath;
            SyrupConfigDirectoryPath = syrupConfigDirectoryPath;
            SyrupWorkDirPath = Path.Combine(SyrupDirPath, Consts.SYRUP_WORK_DIR_NAME);
            SyrupSelfDirPath = Path.Combine(SyrupWorkDirPath, Consts.SELF_SELF_DIR);
            SyrupSelfFilePath = Path.Combine(SyrupWorkDirPath, Consts.SELF_SELF_DIR, Consts.SELF_SELF_FILE);
            SyrupExecutorDirPath = Path.Combine(SyrupWorkDirPath, Consts.EXECUTOR_DIR);
            SyrupExecutorFilePath = Path.Combine(SyrupWorkDirPath, Consts.EXECUTOR_DIR, Consts.EXECUTOR_FILE);
            SyrupNugetsDirPath = Path.Combine(SyrupWorkDirPath, Consts.SYRUP_NUGET_DOWNLOADS_DIR);
            AppsDirPath = Path.Combine(SyrupDirPath, Consts.APP_DIR);
            SyrupTmpDirPath = Path.Combine(SyrupWorkDirPath, Consts.TMP_DIR);
            AppsConfigsDirPath = Path.Combine(configWorkDir, Consts.CONFIG_DIR);
            AppsConfigsForMachineDirPath = Path.Combine(configWorkDir, Consts.CONFIG_DIR, Environment.MachineName);
            AppsLogsDirPath = Path.Combine(configWorkDir, Consts.LOG_DIR);
            Key = config.Key;
            ReleaseInfoUrl = config.ReleaseInfoUrl;
            SyrupReleaseInfoUrl = config.SyrupReleaseInfoUrl;
            Version = AppVersionGenerator.GetVersion();
        }
    }
}