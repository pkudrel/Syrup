using Autofac;
using Syrup.Core.Local.Services;

namespace Syrup.Core.Local.Config
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
           
            builder.RegisterType<FileManagerService>().As<IFileManagerService>();
            builder.RegisterType<LocalReleaseService>().As<ILocalReleaseService>().SingleInstance();
            builder.RegisterType<ScriptService>().As<IScriptService>();
        }
    }
}