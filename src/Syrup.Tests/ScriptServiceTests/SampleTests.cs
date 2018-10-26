using Syrup.Core.Local.Models;
using Syrup.Core.Local.Services;

using Syrup.Core._Infrastructure.Global;
using Xunit;

namespace Syrup.Tests.ScriptServiceTests
{
    public class SampleTests
    {
        private IScriptService _scriptService;

        public SampleTests()
        {
            var workdir = "C:\\Work\\DenebLab\\Syrup\\stuff\\workDir";
            var registry = new Registry(workdir, workdir, new Config());
            _scriptService = new ScriptService(registry);
            
        }

        [Fact]
        public void Run()
        {

            LocalReleaseInfo releaseInfo = new LocalReleaseInfo {App = "VideoAnalyzer"};
          
            _scriptService.RunAfterMakeCurrent(releaseInfo);
        }

        [Fact]
        public void RunBefore()
        {

            LocalReleaseInfo releaseInfo = new LocalReleaseInfo { App = "VideoAnalyzer" };

            _scriptService.RunBeforeMakeCurrent(releaseInfo);
        }
    }
}