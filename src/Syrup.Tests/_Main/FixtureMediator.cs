using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.Variance;
using MediatR;
using Syrup.Core._Infrastructure.Misc;

namespace Syrup.Tests._Main
{
    public abstract class FixtureMediator : IDisposable
    {
        protected FixtureMediator(Action<ContainerBuilder> act)
        {
            var builder = new ContainerBuilder();
         //   BuildAction(builder);
            act(builder);
            AppScope = builder.Build().BeginLifetimeScope();
        }


        protected ILifetimeScope AppScope { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (AppScope != null)
                {
                    AppScope.Dispose();
                    AppScope = null;
                }
            }
        }

        //private static void BuildAction(ContainerBuilder builder)
        //{
        //    builder.RegisterSource(new ContravariantRegistrationSource());
        //    builder.RegisterAssemblyTypes(typeof(IMediator).Assembly)
        //        .AsImplementedInterfaces();

        //    builder.Register<SingleInstanceFactory>(ctx =>
        //    {
        //        var c = ctx.Resolve<IComponentContext>();
        //        return t => c.Resolve(t);
        //    });
        //    builder.Register<MultiInstanceFactory>(ctx =>
        //    {
        //        var c = ctx.Resolve<IComponentContext>();
        //        return t => (IEnumerable<object>) c.Resolve(typeof(IEnumerable<>).MakeGenericType(t));
        //    });

        //    var assembly = typeof(Id).Assembly;

        //    builder.RegisterAssemblyTypes(assembly)
        //        .AsClosedTypesOf(typeof(IRequestHandler<,>))
        //        .AsImplementedInterfaces();

        //    builder.RegisterAssemblyTypes(assembly)
        //        .AsClosedTypesOf(typeof(IAsyncRequestHandler<,>))
        //        .AsImplementedInterfaces();

        //    builder.RegisterAssemblyTypes(assembly)
        //        .AsClosedTypesOf(typeof(INotificationHandler<>))
        //        .AsImplementedInterfaces();

        //    builder.RegisterAssemblyTypes(assembly)
        //        .AsClosedTypesOf(typeof(IAsyncNotificationHandler<>))
        //        .AsImplementedInterfaces();
        //}
    }
}