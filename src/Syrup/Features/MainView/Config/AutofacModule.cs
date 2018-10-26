using Autofac;
using MediatR;
using NLog;
using Syrup.Common.Helpers;
using Syrup.Core._Infrastructure.Global;
using Syrup.Features.MainView.View;

namespace Syrup.Features.MainView.Config
{
    public class AutofacModule : Module
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(ctx => new SingletonFormProvider<MainForm>(() =>
                {
                    var cc = ctx.Resolve<IComponentContext>();
                    var mediator = cc.Resolve<IMediator>();
                    var registry = cc.Resolve<Registry>();
                    return new MainForm(mediator, registry);
                }))
                .AsSelf()
                .SingleInstance();


            builder
                .Register(c => c.Resolve<SingletonFormProvider<MainForm>>().CurrentInstance)
                .As<IFormHandlersContract>()
                .AsSelf();
        }
    }
}