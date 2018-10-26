namespace Syrup.Common.Executor
{
    public class ExecutorRequest
    {
        public ExecutorRequest(string scriptPath, string rootPath, string appPath, string appName)
        {
            ScriptPath = scriptPath;
            RootPath = rootPath;
            AppPath = appPath;
            AppName = appName;
        }


        public string RootPath { get; }
        public string AppPath { get; }
        public string AppName { get; }
        public string ScriptPath { get; set; }
    }
}