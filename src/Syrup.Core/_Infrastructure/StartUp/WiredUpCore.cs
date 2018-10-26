using System.Reflection;
using Autofac;
using NLog;

namespace Syrup.Core._Infrastructure.StartUp
{
    public class WiredUpCore
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly ILifetimeScope _scope;

        public WiredUpCore(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public static void LogAppVersion()
        {
            _log.Info("Application version: {0}", Assembly.GetEntryAssembly().GetName().FullName);
        }


    }
}