using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using Newtonsoft.Json;
using NLog;
using Syrup.Common;
using Syrup.Common.Executor;
using Syrup.Core.Local.Models;
using Syrup.Core.ScriptExecutor;
using Syrup.Core._Infrastructure.Global;

namespace Syrup.Core.Local.Services
{
    public class ScriptService : IScriptService
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private static Assembly _asm;
        private readonly string _pathToApps;
        private readonly Registry _registry;

        public ScriptService(Registry registry)
        {
            _registry = registry;
            _asm = Assembly.LoadFile(registry.SyrupExecutorFilePath);
            _pathToApps = registry.AppsDirPath;
        }

        public SyrupExecuteResult RunAfterMakeCurrent(LocalReleaseInfo releaseInfo)
        {
            _log.Debug($"RunAfterMakeCurrent; File: {releaseInfo.File}");
            var rootPath = new DirectoryInfo(_pathToApps).Parent?.FullName;
            var currentAppPath = Path.Combine(_pathToApps, releaseInfo.App);
            var scriptsPath = Path.Combine(currentAppPath, "_syrup\\scripts");
            var scriptPath = Path.Combine(scriptsPath, "after-make-current.csx");
            var globals = new ExecutorRequest(scriptPath, rootPath, currentAppPath, releaseInfo.Name);
            var res = Run(globals);
            return res;
        }

        public SyrupExecuteResult RunBeforeMakeCurrent(LocalReleaseInfo releaseInfo)
        {
            _log.Debug($"RunBeforMakeCurrent; File: {releaseInfo.File}");
            var rootPath = new DirectoryInfo(_pathToApps).Parent?.FullName;
            var currentAppPath = Path.Combine(_pathToApps, releaseInfo.Name);
            var scriptsPath = Path.Combine(currentAppPath, "_syrup\\scripts");
            var scriptPath = Path.Combine(scriptsPath, "before-make-current.csx");
            var globals = new ExecutorRequest(scriptPath, rootPath, currentAppPath, releaseInfo.Name);
            _log.Debug($"Run; scriptPath: {scriptPath}");
            var res = Run(globals);
            return res;
        }

        private SyrupExecuteResult Run(ExecutorRequest syrupContainer)
        {
            var executeResult = new SyrupExecuteResult();
            string fileString;
            var text = JsonConvert.SerializeObject(syrupContainer);
            var path = _registry.SyrupExecutorFilePath;
            var fileName = $"{Consts.MEM_FILE_PREFIX}-{Guid.NewGuid()}";
            var size = Consts.MEM_FILE_SIZE;

            _log.Debug($"Create mem file; Name: {fileName}; Size: {size}b ");
            _log.Debug($"Script runner: {path}");
            using (var mmf = MemoryMappedFile.CreateNew(fileName, size))
            {
                using (var stream = mmf.CreateViewStream())
                {
                    var writer = new BinaryWriter(stream);
                    writer.Write(text);
                }

                _log.Debug($"Starting script runner: {path}");
                // Command line args are separated by a space
                var p = Process.Start(path, fileName);

                _log.Debug("Waiting child to die");

                p.WaitForExit();
                _log.Debug("Child died");

                using (var stream = mmf.CreateViewStream())
                {
                    var reader = new BinaryReader(stream);
                    fileString = reader.ReadString();
                }
            }

            _log.Debug($"Execute result: {fileString}");
            return executeResult;
        }
    }
}