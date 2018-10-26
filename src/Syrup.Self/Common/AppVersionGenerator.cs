using System;
using System.Reflection;

namespace Syrup.Self.Common
{
    public class VersionGenerator
    {
        /// <summary>
        /// Example:
        /// 0.1.0+BuildCounter.18.Branch.dev.DateTime.2016-07-15T08:37:10Z.Env.local.Sha.a11caeb0db526c1707e8aa40aaec20a315edb119.CommitsCounter.36
        /// </summary>
        /// <returns></returns>
        public static Version GetVersion()
        {
            var w = new Worker();
            return w.Make();
        }
        public static Version GetVersion(Assembly assembly)
        {
            var w = new Worker(assembly);
            return w.Make();
        }


        internal class Worker
        {
            private readonly Assembly _assembly;

            public Worker(Assembly assembly)
            {
                _assembly = assembly;
            }

            public Worker()
            {
                _assembly = Assembly.GetEntryAssembly();
            }

            public string GetAssemblyVersion => _assembly.GetName().Version.ToString();

            public Version Make()
            {
                var name = _assembly.GetName();
                var assemblyVersion = name.Version.ToString();
                var appName = name.Name;

                var assemblyFileVersion = GetAttribute<AssemblyFileVersionAttribute>()?.Version;
                var pv = GetAttribute<AssemblyInformationalVersionAttribute>();
                var res = new Version(assemblyVersion, assemblyFileVersion, appName);
                var i = pv?.InformationalVersion;
                appName = GetAttribute<AssemblyProductAttribute>()?.Product;
                if (i == null) return res;
                var parts = i.Split('+');
                if (parts.Length != 2) return res;
                var sem = parts[0];
                var info = new Info(parts[1]);

                return new Version(assemblyVersion, assemblyFileVersion, appName,
                    parts[1], sem,
                    info.Get("BuildCounter"), info.Get("Branch"), info.Get("DateTime"),
                    info.Get("Env"), info.Get("Sha"), info.Get("CommitsCounter")
                    );
            }


            private T GetAttribute<T>() where T : class
            {

                var t = Attribute.GetCustomAttribute(_assembly, typeof(T), false);
                return t as T;
            }

            internal class Info
            {
                private readonly string[] _arr;
                private readonly int _max;

                public Info(string s)
                {
                    _arr = s.Split('.');
                    _max = _arr.GetUpperBound(0) - 1;
                }

                public string Get(string name)
                {
                    var i = Array.FindIndex(_arr, t => t.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (i < 0) return string.Empty;
                    if (i > _max) return string.Empty;
                    return _arr[i + 1];
                }
            }
        }
    }

    public class Version
    {
        public Version(string assemblyVersion, string assemblyFileVersion, string assemblyName)
        {
            AssemblyVersion = assemblyVersion;
            AssemblyFileVersion = assemblyFileVersion;
            AssemblyName = assemblyName;
        }

        public Version(string assemblyVersion, string assemblyFileVersion, string assemblyName,
            string assemblyProductVersion, string semVer, string buildCounter,
            string branch, string dateTime, string env, string sha, string commitsCounter)
        {
            AssemblyVersion = assemblyVersion;
            AssemblyFileVersion = assemblyFileVersion;
            AssemblyName = assemblyName;
            AssemblyProductVersion = assemblyProductVersion;
            SemVer = semVer;
            BuildCounter = buildCounter;
            Branch = branch;
            DateTime = dateTime;
            Env = env;
            Sha = sha;
            CommitsCounter = commitsCounter;
        }

        public string AssemblyVersion { get; }
        public string AssemblyFileVersion { get; }
        public string AssemblyName { get; set; }
        public string AssemblyProductVersion { get; }
        public string SemVer { get; }
        public string BuildCounter { get; }
        public string Branch { get; }
        public string DateTime { get; }
        public string Env { get; }
        public string Sha { get; }
        public string CommitsCounter { get; }

        public string MainVersion => string.IsNullOrEmpty(SemVer) ? AssemblyFileVersion : SemVer;
        public string FullName => $"{AssemblyName} {MainVersion}";
        public string FullInfo => string.IsNullOrEmpty(AssemblyProductVersion) ? FullName : $"{AssemblyName} {AssemblyProductVersion}";

        public override string ToString()
        {
            return MainVersion;
        }
    }
}