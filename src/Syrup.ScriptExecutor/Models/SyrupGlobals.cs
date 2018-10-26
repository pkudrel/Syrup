using System;
using System.IO.Compression;
using System.Net;
using NLog;
using Syrup.ScriptExecutor.Services;

namespace Syrup.ScriptExecutor.Models
{
    public class SyrupGlobals
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();


        public SyrupGlobals(string currentRootPath, string currentAppPath, string currentAppName)
        {
            CurrentRootPath = currentRootPath;
            CurrentAppPath = currentAppPath;
            CurrentAppName = currentAppName;
            ExecuteResult = new SyrupExecuteResult();
        }


        public SyrupExecuteResult ExecuteResult { get; }
        public string CurrentRootPath { get; }
        public string CurrentAppPath { get; }
        public string CurrentAppName { get; }

        public void CreateShortcut(string targetFile, string destinationFile, string description)
        {
            var sl = new ShellLink
            {
                Target = targetFile,
                Description = description
            };

            sl.Save(destinationFile);
        }

        public void CreateShortcut(string targetFile, string workingDirectory, string destinationFile,
            string description)
        {
            var sl = new ShellLink
            {
                Target = targetFile,
                Description = description,
                WorkingDirectory = workingDirectory
            };

            sl.Save(destinationFile);
        }

        public void ExtractZipFile(string zipPath, string extractPath)
        {
            _log.Debug($"ExtractZipFile: Src: {zipPath}; Dst: {extractPath}");

            try
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            _log.Debug($"ExtractZipFile: End");
        }


        public void DownloadFile(string url, string path)
        {
            _log.Debug($"Download: Src: {url}; Dst: {path}");
            var webClient = new WebClient();
            webClient.DownloadProgressChanged += (s, e) => { Console.Write("."); };
            webClient.DownloadFileCompleted += (s, e) =>
            {
                Console.WriteLine();
                Console.WriteLine("Complete");
                // any other code to process the file
            };

            try
            {
                Console.WriteLine($"Download src: {url}");
                Console.WriteLine($"Download dst: {path}");
                webClient.DownloadFile(url, path);
            }
            catch (WebException e)
            {
                _log.Error(e);
            }

            _log.Debug($"Download: End");
            webClient.Dispose();
        }
    }
}