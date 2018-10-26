using System;

namespace Syrup.Core.Local.Models
{
    public class LocalReleaseInfo
    {
        public string App { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
        public string Sha { get; set; }
        public string FileUrl { get; set; }
        public string SemVer { get; set; }
        public string Channel { get; set; }
        public DateTime RelaseDate { get; set; }
        public bool IsOnServer { get; set; }
        public bool IsLocal { get; set; }
        public bool IsLocalNuget { get; set; }
        public bool IsActive { get; set; }
        public bool IsValid { get; set; }
        public bool IsExtracted { get; set; }
        public string Message { get; set; }
    }
}