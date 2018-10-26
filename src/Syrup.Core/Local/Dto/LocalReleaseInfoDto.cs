namespace Syrup.Core.Local.Dto
{
    public class LocalReleaseInfoDto
    {
        public string App { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
        public string RelaseDate { get; set; }
        public bool IsOnServer { get; set; }
        public bool IsLocal { get; set; }
        public bool IsActive { get; set; }
        public bool IsValid { get; set; }
        public bool IsExtracted { get; set; }
        public bool IsLocalNuget { get; set; }
    }
}   