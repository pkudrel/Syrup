using System;
using Autofac;

namespace Syrup.Tests._Main
{
    public abstract class FixtureCleanAutofac : IDisposable
    {
        protected FixtureCleanAutofac(Action<ContainerBuilder> act)
        {
            var builder = new ContainerBuilder();
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
    }
}