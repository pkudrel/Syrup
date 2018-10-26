using System.Collections.Generic;

namespace Syrup.ScriptExecutor.Models
{
    public class SyrupExecuteResult
    {
        public bool Continue { get; private set; }

        public List<string> Messages { get; }

        public SyrupExecuteResult()
        {
            Continue = true;
            Messages = new List<string>();
        }

        public void AddMessage(string message)
        {
            Messages.Add(message);
        }

        public void AbortProcess(string message)
        {
            Continue = false;
            Messages.Add(message);

        }
    }
}