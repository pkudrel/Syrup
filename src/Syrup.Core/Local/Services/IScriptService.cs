using Syrup.Core.Local.Models;
using Syrup.Core.ScriptExecutor;

namespace Syrup.Core.Local.Services
{
    public interface IScriptService
    {
        SyrupExecuteResult RunAfterMakeCurrent(LocalReleaseInfo releaseInfo);
        SyrupExecuteResult RunBeforeMakeCurrent(LocalReleaseInfo releaseInfo);
    }


}