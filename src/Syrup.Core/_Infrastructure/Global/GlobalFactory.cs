using System;
using System.IO;
using Newtonsoft.Json;
using NLog;
using Syrup.Common;
using Syrup.Common.Bootstrap;

namespace Syrup.Core._Infrastructure.Global
{
    public class GlobalFactory
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();


        public Tuple<Registry, Config> GetGlobal()
        {
            var env = AppEnvironmentBuilder.Instance.GetAppEnvironment();
            var configDir = env.IsDeveloperMode ? env.RootDir : env.ExeFileDir;
            var config = GetConfig(configDir);
            var registry = GetRegistry(config, env);
            _log.Info($"Application version: {registry.Version.FullInfo}");
            return new Tuple<Registry, Config>(registry, config);
        }

        private Registry GetRegistry(Config config, AppEnvironment env)
        {
            var registry = new Registry(env, config);

            Common.Io.Misc.CreateDirIfNotExist(registry.SyrupWorkDirPath);
            Common.Io.Misc.CreateDirIfNotExist(registry.SyrupNugetsDirPath);
            Common.Io.Misc.CreateDirIfNotExist(registry.AppsDirPath);
            Common.Io.Misc.CreateDirIfNotExist(registry.SyrupTmpDirPath);
            Common.Io.Misc.CreateDirIfNotExist(registry.AppsLogsDirPath);
            Common.Io.Misc.CreateDirIfNotExist(registry.AppsConfigsDirPath);
            Common.Io.Misc.CreateDirIfNotExist(registry.AppsConfigsForMachineDirPath);

            return registry;
        }

        private static Config GetConfig(string configDir)
        {
            var path = Path.Combine(configDir, Consts.SYRUP_CONFIG_FILE);
            if (File.Exists(path) == false)
                throw new FileNotFoundException($"Config file not found: {path}");
            var c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            return c;
        }
    }
}