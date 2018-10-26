using Syrup.Self.Common;

namespace Syrup.Self.Parts.Globals
{
    public class Registry
    {
        public string SyrupDirPath { get; set; }
        public string SyrupAppPath { get; set; }
        public string SyrupTmpDirPath { get; set; }
        public Version CurrentAssemblyVersion { get; set; }
    }
}