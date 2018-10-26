using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Syrup.Self.Common;
using Syrup.Self.Parts.Globals;

namespace Syrup.Self.Parts.Log
{
    public sealed class Logger
    {
        private readonly List<string> _messages;
        private readonly string _pathToLogDir;

        private readonly string _pathToLogFile;
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Logger()
        {
        }

        private Logger()
        {
            _messages = new List<string>();
            var currentDate = DateTime.UtcNow;
            _pathToLogDir = GetPathToLog();
            var fileNameSubName = currentDate.ToString("yyyy-MM-ddTHH.mm.ssZ");
            _pathToLogFile = Path.Combine(_pathToLogDir, $"log-{fileNameSubName}.txt");
        }

        public static Logger Instance { get; } = new Logger();


        private void CleanLogDir()
        {
            var di = new DirectoryInfo(_pathToLogDir);
            var files = di.GetFiles("*.txt");
            var toDelete = files.OrderByDescending(x => x.CreationTimeUtc).Skip(20);
            foreach (var fileInfo in toDelete)
                try
                {
                    File.Delete(fileInfo.FullName);
                }
                catch (Exception)
                {
                    // ignored
                }
        }

        public void Save()
        {
            File.WriteAllLines(_pathToLogFile, _messages, Encoding.UTF8);
            CleanLogDir();
        }

        private static string GetPathToLog()
        {
            var syrupDir = SyrupAndAppFinder.Instance.SyrupDirectoryPath;
            var machine = Environment.MachineName;
            var path = Path.Combine(syrupDir, "log", machine, Consts.SYRUP_LOG_DIR);
            Io.CreateDirIfNotExist(path);
            return path;
        }

        public void Info(string message)
        {
            _messages.Add($"{DateTime.UtcNow:o}|INFO|{message}");
        }

        public void Error(string message)
        {
            _messages.Add($"{DateTime.UtcNow:o}|ERROR|{message}");
        }

        public void Debug(string message)
        {
            _messages.Add($"{DateTime.UtcNow:o}|DEBUG|{message}");
        }
    }
}