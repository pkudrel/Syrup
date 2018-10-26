using Syrup.Common.Executor;

namespace Syrup.Core.ScriptExecutor
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