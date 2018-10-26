using System;

namespace Syrup.Core.Server.Models
{
    public class SyrupFileInfo
    {
        public string App { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
        public string Sha { get; set; }
        public string SemVer { get; set; }
        public DateTime RelaseDate { get; set; }
    }
}