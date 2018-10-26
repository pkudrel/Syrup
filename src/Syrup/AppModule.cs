using System;
using System.IO;
using Autofac;
using MediatR;
using NLog;
using Syrup.Common.Bootstrap;
using Syrup.Common.Io;

namespace Syrup
{
    public class AppModule : Module
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();


        protected override void Load(ContainerBuilder builder)
        {
            CreateConfigAndRegistry(builder);


        }

        private static void CreateConfigAndRegistry(ContainerBuilder builder)
        {
            //var env = Boot.Instance.GetAppEnvironment();
            //var configDirPath = env.ConfigDir;
            //var configPath = Path.Combine(configDirPath, "");

            //Misc.CreateDirIfNotExist(configDirPath);

        }


    }
}