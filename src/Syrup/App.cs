using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autofac;
using MediatR;
using NLog;
using Syrup.Common;
using Syrup.Common.Bootstrap;
using Syrup.Core;
using Syrup.Features.MainView.View;
using AppStartedEvent = Syrup.Common.App.ReqRes.AppStartedEvent;
using AppStartingEvent = Syrup.Common.App.ReqRes.AppStartingEvent;

namespace Syrup
{
    /// <summary>
    /// Two important functions:
    /// 1. MainLowLevel: Check if is only one instance, create extended registry, add assemblies, etc. Any errors should be
    /// handled in this file. No domain configuration in any form. After this function program must have ExtendedRegistry
    /// object and well initialized file log
    /// 2. MainAsync: Registration all AutoFac modules, domain configuration, start main user code
    /// Scenario:
    /// a) Registration of all AutoFac modules (module code per features should be in feature/config folder (by
    /// convention).
    /// Exception: AppModule.cs - in root. In this file you should crate Registry and other important domain objects
    /// b) Publishing 'AppStartingEvent' with Mediator. Domain initialization and checks. Handlers should be in
    /// feature/handlers folder (by convention)
    /// Exception: AppHandlers.cs - in root. Important checks and  initializations
    /// </summary>
    internal static class App
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            Boot.Instance.Start(typeof(App).Assembly);
            Boot.Instance.AddAssembly(typeof(AppConst).Assembly, AssemblyInProject.Core);
            Boot.Instance.AddAssembly(typeof(Consts).Assembly, AssemblyInProject.Common);

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(Boot.Instance.GetAssemblies());
            try
            {
                using (var container = builder.Build())
                {
                    using (var scope = container.BeginLifetimeScope())
                    {
                        _log.Info($"Aplication: {Boot.Instance.GetAppEnvironment().AppVersion.FullName}");
                        var mediator = scope.Resolve<IMediator>();
                        // Any configuration checks, initializations should be handled by this event
                        // Most important start code is in the AppHandlers class. 
                        // You may extend it as you wont. 
                        await mediator.Publish(new AppStartingEvent());

                        // And after 
                        await mediator.Publish(new AppStartedEvent());
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        var view = scope.Resolve<MainForm>();
                        view.Closing += (sender, args2) => { _log.Debug("Closing"); };
                        Application.Run(view);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                _log.Error(e.Message);
                _log.Error(e.InnerException?.Message);
                MessageBox.Show(e.Message + e.InnerException?.Message, "Critical ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw e;
            }
        }
    }
}