using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NLog;
using NLog.Config;
using NLog.Windows.Forms;
using Syrup.Common;
using Syrup.Common.Bootstrap;
using Syrup.Common.Io;
using Syrup.Core._Infrastructure.Global;
using AppStartingEvent = Syrup.Common.App.ReqRes.AppStartingEvent;

namespace Syrup
{
    public class AppHandlers : INotificationHandler<AppStartingEvent>
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly Registry _registry;

        public AppHandlers(Registry registry)
        {
            _registry = registry;
        }

        public Task Handle(AppStartingEvent notification, CancellationToken cancellationToken)
        {
            InitNlog();
            ExtractEmbed();
            return Task.CompletedTask;
        }

        private static void InitNlog()
        {
            ConfigurationItemFactory
                .Default
                .Targets
                .RegisterDefinition("FormControl", typeof(FormControlTarget));

            const string formWinformTarget = "form-winform";


            var config = LogManager.Configuration;

            // winforms logger 
            var formControlTarget = new FormControlTarget
            {
                Name = formWinformTarget,
                Layout = "${message}${newline}",
                Append = true,
                ReverseOrder = false,
                ControlName = "LogBox",
                FormName = "MainForm"
            };
            config.AddTarget(formControlTarget);
            config.AddRule(LogLevel.Info, LogLevel.Info, formControlTarget, "logUi");

            LogManager.Configuration = config;
        }

        public void ExtractEmbed()
        {
            var asm = AssemblyCollector.Instance.GetMainAssembly();
            ExtractEmbedSelf(asm);
            ExtractEmbedExecutor(asm);
        }

        private void ExtractEmbedSelf(Assembly asm)
        {
            var rsn = asm.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(Consts.SELF_EMBED));
            Misc.CreateDirIfNotExist(_registry.SyrupSelfDirPath);
            var bytes = GetBytes(asm, rsn);
            File.WriteAllBytes(_registry.SyrupSelfFilePath, bytes);
        }

        private void ExtractEmbedExecutor(Assembly asm)
        {
            var rsn = asm.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(Consts.EXECUTOR_EMBED));
            Misc.CreateDirIfNotExist(_registry.SyrupExecutorDirPath);
            var bytes = GetBytes(asm, rsn);
            File.WriteAllBytes(_registry.SyrupExecutorFilePath, bytes);
        }

        private static byte[] GetBytes(Assembly asm, string rsn)
        {
            using (var stream = asm.GetManifestResourceStream(rsn))
            {
                if (stream == null) return null;
                var ba = new byte[stream.Length];
                stream.Read(ba, 0, ba.Length);
                return ba;
            }
        }
    }
}