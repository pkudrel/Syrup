using System.IO;
using System.Reflection;
using Syrup.Self.Common;

namespace Syrup.Self.Parts.Globals
{
    public class GlobalFactory
    {
        public Config GetConfig()
        {
            var syrupDir = SyrupAndAppFinder.Instance.SyrupDirectoryPath;
            var configFilePath = Path.Combine(syrupDir, Consts.SYRUP_CONFIG_FILE);
            var json = File.ReadAllText(configFilePath);
            var config = JsonSerializer<Config>.DeSerialize(json);
            return config;
        }

        public Registry GetRegistry()
        {
            var syrupDir = SyrupAndAppFinder.Instance.SyrupDirectoryPath;
            var syrupAppPath = Path.Combine(syrupDir, Consts.SYRUP_FILE);
            var tmpDirPath = Path.Combine(syrupDir, Consts.SYRUP_WORK_DIR, Consts.SYRUP_TMP_DIR);
            var version = GetVersion(syrupAppPath);
            return new Registry
            {
                CurrentAssemblyVersion = version,
                SyrupAppPath = syrupAppPath,
                SyrupDirPath = syrupDir,
                SyrupTmpDirPath = tmpDirPath
            };
        }

        private Version GetVersion(string pathToAssembly)
        {
            var assembly = Assembly.Load(File.ReadAllBytes(pathToAssembly));
            var version = VersionGenerator.GetVersion(assembly);
            return version;
        }
    }
}