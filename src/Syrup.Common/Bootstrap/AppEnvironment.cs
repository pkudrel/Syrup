﻿namespace Syrup.Common.Bootstrap
{
    public class AppEnvironment
    {
        public string WorkDir { get; set; }
        public string AssemblyName { get; set; }
        public string AssemblyFilePath { get; set; }
        public string ExeFileDir { get; set; }
        public string SyrupDir { get; set; }
        public bool IsSyrup { get; set; }
        public string ConfigDir { get; set; }
        public string VarDir { get; set; }
        public string GlobalDir { get; set; }
        public bool IsDebug { get; set; }
        public string GitDir { get; set; }
        public string RootDir { get; set; }
        public string RepositoryRootDir { get; set; }
        public string LogDir { get; set; }
        public string MachineName { get; set; }
        public string ProcessorArchitecture { get; set; }
        public string DevDir { get; set; }
        public AppVersion AppVersion { get; set; }
        public bool IsDeveloperMode { get; set; }
        public bool Is64BitProcess { get; set; }
        public string[] CommandLineArgs { get; set; }
    }
}