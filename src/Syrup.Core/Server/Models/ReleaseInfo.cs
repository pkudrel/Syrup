using System;

namespace Syrup.Core.Server.Models
{
    public class ReleaseInfo
    {
        public string App { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
        public string Sha { get; set; }
        public string FileUrl { get; set; }
        public string SemVer { get; set; }
        public string Channel { get; set; }
        public DateTime RelaseDate { get; set; }
        public bool Active { get; set; }
    }
}