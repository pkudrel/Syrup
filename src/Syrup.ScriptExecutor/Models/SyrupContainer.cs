namespace Syrup.ScriptExecutor.Models
{
    public class SyrupContainer
    {
        public SyrupContainer(SyrupGlobals globals)
        {
            Syrup = globals;
        }

        public SyrupGlobals Syrup { get; }
    }
}