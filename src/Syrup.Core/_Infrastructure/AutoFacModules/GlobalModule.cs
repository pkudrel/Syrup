using Autofac;
using Syrup.Core._Infrastructure.Global;

namespace Syrup.Core._Infrastructure.AutoFacModules
{
    public class GlobalModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var gf = new GlobalFactory();
            var rc = gf.GetGlobal();
            builder.RegisterInstance(rc.Item1).SingleInstance().AsSelf();
            builder.RegisterInstance(rc.Item2).SingleInstance().AsSelf();
        }
    }
}