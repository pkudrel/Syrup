using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Syrup.Common.Executor;
using Syrup.ScriptExecutor.Bootstrap;
using Syrup.ScriptExecutor.Services;

namespace Syrup.ScriptExecutor
{
    internal class Program
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();


        private static void RunMemoryMappedFile(string fileName)
        {
            try
            {
                using (var mmf = MemoryMappedFile.OpenExisting(fileName))
                {
                    string requestJson;
                    using (var stream = mmf.CreateViewStream())
                    {
                        var reader = new BinaryReader(stream);
                        requestJson = reader.ReadString();
                    }

                    _log.Debug($"Request: {requestJson}");
                    var request = JsonConvert.DeserializeObject<ExecutorRequest>(requestJson);
                    var runner = new ScriptRunner();
                    var response = runner.Run(request);

                    using (var input = mmf.CreateViewStream())
                    {
                        var writer = new BinaryWriter(input);
                        var json = JsonConvert.SerializeObject(response);
                        writer.Write(json);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                _log.Error(e.Message);
                _log.Error(e.InnerException?.Message);
            }
        }

        private static void Main(string[] args)
        {
            Boot.Instance.Start(typeof(Program).Assembly);

            if (args.Length > 0)
            {
                var fileName = args[0];
                RunMemoryMappedFile(fileName);
            }
            else
            {
                RunFromString();
            }

            _log.Debug("Done;");
        }


        private static void RunFromString()
        {
            var requestJson = @"{""RootPath"":""C:\\work\\DenebLab\\Syrup\\.dev\\.app.vs"",
                                ""AppPath"":""C:\\work\\DenebLab\\Syrup\\.dev\\.app.vs\\app\\LukeSearch.1.0.709"",
                                ""AppName"":""LukeSearch.1.0.709"",
                                ""ScriptPath"":""C:\\work\\DenebLab\\Syrup\\.dev\\.app.vs\\app\\LukeSearch.1.0.709\\_syrup\\scripts\\before-make-current.csx""}";
            var request = JsonConvert.DeserializeObject<ExecutorRequest>(requestJson);
            var runner = new ScriptRunner();
            var response = runner.Run(request);
        }

        private static async Task MainAsync(string[] args)
        {
            var mmfName = args[0];
            Boot.Instance.Start(typeof(Program).Assembly);
            try
            {
                using (var mmf = MemoryMappedFile.OpenExisting(mmfName))
                {
                    string fileString;
                    using (var stream = mmf.CreateViewStream())
                    {
                        var reader = new BinaryReader(stream);
                        fileString = reader.ReadString();
                    }

                    _log.Debug($"File value: {fileString}");
                    using (var input = mmf.CreateViewStream())
                    {
                        var writer = new BinaryWriter(input);
                        writer.Write(mmfName);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                _log.Error(e.Message);
                _log.Error(e.InnerException?.Message);
            }
        }
    }
}