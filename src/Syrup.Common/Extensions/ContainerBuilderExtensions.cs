using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Syrup.Common.Mediator;

namespace Syrup.Common.Extensions
{
    public static class ContainerBuilderExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TDecorater"></typeparam>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="builder"></param>
        /// <param name="serviceAction"></param>
        /// <param name="decoratorAction"></param>
        /// <remarks>
        ///   //builder
        //    .RegisterDecorator
        //    <SimpleDocumentAnalyzerHandler, LoggingDecorator, INotificationHandler<BrowserCreatedSimpleDocumentMessage>>(
        //        s => s.InstancePerLifetimeScope(),
        //        d => d.InstancePerLifetimeScope());
        /// 
        /// </remarks>
        public static void RegisterDecorator<TService, TDecorater, TInterface>(this ContainerBuilder builder,
            Action<IRegistrationBuilder<TService, ConcreteReflectionActivatorData, SingleRegistrationStyle>>
                serviceAction,
            Action<IRegistrationBuilder<TDecorater, ConcreteReflectionActivatorData, SingleRegistrationStyle>>
                decoratorAction)
        {
            var serviceBuilder = builder
                .RegisterType<TService>()
                .Named<TInterface>(typeof(TService).Name);

            serviceAction(serviceBuilder);

            var decoratorBuilder =
                builder.RegisterType<TDecorater>()
                    .WithParameter(
                        (p, c) => p.ParameterType == typeof(TInterface),
                        (p, c) => c.ResolveNamed<TInterface>(typeof(TService).Name))
                    .As<TInterface>();

            decoratorAction(decoratorBuilder);
        }

        public static IPreRequestHandler<TServicePre>[] ResolvePreRequest<TServicePre>(this IComponentContext ctx)
        {
            var cc = ctx.Resolve<IComponentContext>();
            var pre = cc.Resolve<IEnumerable<IPreRequestHandler<TServicePre>>>().ToArray();
            return pre;
        }
        public static IPostRequestHandler<TServicePre, TServicePost>[] ResolvePostRequest<TServicePre, TServicePost>(this IComponentContext ctx)
        {
            var cc = ctx.Resolve<IComponentContext>();
            var post = cc.Resolve<IEnumerable<IPostRequestHandler<TServicePre, TServicePost>>>().ToArray();
            return post;
        }
    }
}