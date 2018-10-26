using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Syrup.Self.Parts.Globals
{


    public sealed class SyrupAndAppFinder
    {
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static SyrupAndAppFinder()
        {
        }

        private SyrupAndAppFinder()
        {
        }

        public string SyrupDirectoryPath => GetSyrupDir();


        public string AppDirectoryPath
        {
            get
            {
                var configDirPath =
                    (Environment.MachineName == "DUKE")
                    || (Environment.MachineName == "DELL-780")
                        ? "C:\\Work\\DenebLab\\Syrup\\stuff\\workDir\\.syrup\\updater"
                        : AppDirPath;
                return configDirPath;
            }
        }

        private string AppDirPath
            =>
            Path.GetDirectoryName(
                Uri.UnescapeDataString(new UriBuilder(Assembly.GetEntryAssembly().CodeBase).Path));

        public static SyrupAndAppFinder Instance { get; } = new SyrupAndAppFinder();

        private string GetSyrupDir()
        {
            var di = new DirectoryInfo(AppDirectoryPath);
            if (di.Parent == null) throw new FileNotFoundException($"Cannot find: {Consts.SYRUP_FILE}");
            if (di.Parent.Parent == null) throw new FileNotFoundException($"Cannot find: {Consts.SYRUP_FILE}");
            var syrupDirDi = di.Parent.Parent;
            var isSyrupIn = syrupDirDi.GetFiles(Consts.SYRUP_FILE).Any();
            var isSyrupConfigIn = syrupDirDi.GetFiles(Consts.SYRUP_CONFIG_FILE).Any();
            if (!isSyrupIn || !isSyrupConfigIn)
                throw new FileNotFoundException($"Cannot find: {Consts.SYRUP_FILE} or {Consts.SYRUP_CONFIG_FILE}");

            return syrupDirDi.FullName;
        }
    }
}