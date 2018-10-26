using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using NLog;
using Syrup.Common.Executor;
using Syrup.ScriptExecutor.Models;

namespace Syrup.ScriptExecutor.Services
{
    public class ScriptRunner
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public ExecutorResponse Run(ExecutorRequest executorRequest)
        {
            var ret = new ExecutorResponse();
            var sw = new Stopwatch();
            sw.Start();
            _log.Debug($"Run script: {executorRequest.ScriptPath}");
            var sg = new SyrupGlobals(executorRequest.RootPath, executorRequest.AppPath, executorRequest.AppName);
            _log.Debug($"AppName: {sg.CurrentAppName}");
            _log.Debug($"RootPath: {sg.CurrentRootPath}");
            _log.Debug($"AppPath: {sg.CurrentAppPath}");
   
            var syrupContainer = new SyrupContainer(sg);
            var executeResult = syrupContainer.Syrup.ExecuteResult;
            var code = File.ReadAllText(executorRequest.ScriptPath);
            //_log.Debug($"Code: {code}");

            try
            {
                var opts = ScriptOptions.Default
                        .AddReferences(Assembly.GetEntryAssembly())
                        .AddImports("System")
                        .AddImports("System.Diagnostics")
                        .AddImports("System.Linq")
                        .AddImports("Syrup.ScriptExecutor.Models")
                    ;

                var script = CSharpScript.Create(code, opts, typeof(SyrupContainer));
                var compilation = script.GetCompilation();
                var diagnostics = compilation.GetDiagnostics();

                var r1 = compilation.ReferencedAssemblyNames
                    .Where(x => x.Name.IndexOf("syrup", StringComparison.OrdinalIgnoreCase) > -1)
                    .ToArray();
                foreach (var assemblyIdentity in r1) _log.Debug($"assemblyIdentity: {assemblyIdentity}");


                if (diagnostics.Any())
                {
                    foreach (var diagnostic in diagnostics)
                    {
                        var msg = diagnostic.GetMessage();
                        executeResult.AddMessage(msg);
                        Console.WriteLine(diagnostic.GetMessage());
                    }

                    executeResult.AbortProcess("Script error");
                }
                else
                {
                    var result = script.RunAsync(syrupContainer).Result;
                    ret.ReturnValue = result.ReturnValue?.ToString() ?? string.Empty;
                    ret.Success = true;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            ret.Messages = executeResult.Messages;
            ret.Continue = executeResult.Continue;
            sw.Stop();
            _log.Debug($"Run script took: {sw.ElapsedMilliseconds}ms");
            return ret;
        }
    }
}