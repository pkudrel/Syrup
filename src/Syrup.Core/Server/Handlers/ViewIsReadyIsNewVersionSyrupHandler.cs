using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NLog;
using Syrup.Common;
using Syrup.Common.Version;
using Syrup.Core.Global.Events;
using Syrup.Core.Server.Logic;
using Syrup.Core.Server.Models;
using Syrup.Core.Server.ReqRes;
using Syrup.Core._Infrastructure.Global;

namespace Syrup.Core.Server.Handlers
{
    public class ViewIsReadyIsNewVersionSyrupHandler : INotificationHandler<ViewIsReadyAsyncEvent>
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private static readonly Logger _logCtrl = LogManager.GetLogger(Consts.LOG_UI);
        private readonly IMediator _mediator;
        private readonly Registry _registry;
        private readonly IReleaseInfoService _releaseInfoService;

        public ViewIsReadyIsNewVersionSyrupHandler(IMediator mediator, Registry registry,
            IReleaseInfoService releaseInfoService)
        {
            _mediator = mediator;
            _registry = registry;
            _releaseInfoService = releaseInfoService;
        }

        public async Task Handle(ViewIsReadyAsyncEvent notification, CancellationToken cancellationToken)
        {
            await Task.Delay(700, cancellationToken);
            _logCtrl.Info($"Begin donload syrup release info form: {_registry.SyrupReleaseInfoUrl}");
            var r = await _releaseInfoService.GetReleaseInfoAsync(_registry.SyrupReleaseInfoUrl);
            var releaseInfo = r.OrderByDescending(x => SemVersion.Parse(x.SemVer)).FirstOrDefault();
            if (releaseInfo != null)
            {
                var version = GetLocalSemVersion(_registry.Version);
                if (IsUpdateNeeded(version, releaseInfo))
                {
                    var res = await _mediator.Send(new MakeUpdatePanelVisibleRequest($"{releaseInfo.App} {releaseInfo.SemVer}"), cancellationToken);
                }
            }
        }

        private static string GetLocalSemVersion(Version version)
        {
            if (string.IsNullOrEmpty(version.SemVer))
                return version.AssemblyVersion.Substring(0, 5);
            return version.SemVer;
        }


        private static bool IsUpdateNeeded(string currentSemVersion, ReleaseInfo releaseInfo)
        {

            return true;
            _logCtrl.Info($"Is Update Needed??");
            var currentSem = SemVersion.Parse(currentSemVersion);
            var newestSem = SemVersion.Parse(releaseInfo.SemVer);
            _logCtrl.Info($"Current local syrup version: {currentSem}");
            _logCtrl.Info($"Newest version on server: {newestSem}");

            if (currentSem < newestSem)
            {
                _logCtrl.Info($"Update is needed {currentSem} ===> {newestSem}");
                return true;
            }
            if (currentSem == newestSem)
            {
                _logCtrl.Info($"Update not needed - same versions");
                return false;
            }

            _logCtrl.Info($"Update not needed - local version is older");
            return false;
        }
    }
}