using Syrup.Common.Executor;
using Syrup.Core.ScriptExecutor;

namespace Syrup.Tests.ScriptServiceTests
{
    public class SyrupContainer
    {
        public SyrupContainer(ExecutorRequest globals)
        {
            Syrup = globals;
        }

        public ExecutorRequest Syrup { get; }
    }
}