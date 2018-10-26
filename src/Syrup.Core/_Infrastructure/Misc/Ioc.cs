using System;
using System.Reflection;
using Autofac;
using NLog;

namespace Syrup.Core._Infrastructure.Misc
{
    public static class Ioc
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private static readonly Lazy<IContainer> _lazy = new Lazy<IContainer>(Build);

        public static IContainer Container => _lazy.Value;


        private static IContainer Build()
        {
            _log.Debug("Build container");
            var builder = new ContainerBuilder();
            var assembly = Assembly.GetExecutingAssembly();
            builder.RegisterAssemblyModules(assembly);

            return builder.Build();
        }
    }
}