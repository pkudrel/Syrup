using Autofac;
using Syrup.Core.Server.Logic;
using Syrup.Core._Infrastructure.Misc;

namespace Syrup.Core.Server.Config
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ReleaseInfoService>().As<IReleaseInfoService>();
            builder.Register(ctx =>
            {
                var cc = ctx.Resolve<IComponentContext>();
                var c = cc.Resolve<_Infrastructure.Global.Config>();
                var r = Utility.CreateWebClient(c.Key);
                return new FileDownloader( () => r);
            }).As<IFileDownloader>().InstancePerDependency();
        }
    }
}