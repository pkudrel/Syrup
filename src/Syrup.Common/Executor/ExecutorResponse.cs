using System.Collections.Generic;

namespace Syrup.Common.Executor
{
    public class ExecutorResponse
    {
        public string ReturnValue { get; set; }

        public bool Continue { get; set; }

        public List<string> Messages { get; set; }
        public bool Success { get; set; }
    }
}