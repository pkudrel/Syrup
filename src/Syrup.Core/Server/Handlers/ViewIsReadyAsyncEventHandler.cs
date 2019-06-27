using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NLog;
using Syrup.Common;
using Syrup.Core.Global.Events;
using Syrup.Core.Local.Events;
using Syrup.Core.Local.Services;
using Syrup.Core.Server.Events;
using Syrup.Core.Server.Logic;
using Syrup.Core._Infrastructure.Global;

namespace Syrup.Core.Server.Handlers
{
    public class ViewIsReadyAsyncEventHandler : INotificationHandler<ViewIsReadyAsyncEvent>
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private static readonly Logger _logCtrl = LogManager.GetLogger(Consts.LOG_UI);
        private readonly IFileManagerService _fileManagerService;

        private readonly IMediator _mediator;
        private readonly Registry _registry;
        private readonly IReleaseInfoService _releaseInfoService;

        public ViewIsReadyAsyncEventHandler(IMediator mediator, Registry registry,
            IReleaseInfoService releaseInfoService,
            IFileManagerService fileManagerService)
        {
            _mediator = mediator;
            _registry = registry;
            _releaseInfoService = releaseInfoService;
            _fileManagerService = fileManagerService;
        }

        public async Task Handle(ViewIsReadyAsyncEvent notification, CancellationToken cancellationToken)
        {
            await Task.Delay(800, cancellationToken);
            await _mediator
                .Publish(new LongProcessStartedEvent("Get releases infos ... ",
                    int.MinValue,
                    LongProcessType.IsIndeterminate), cancellationToken);

            _logCtrl.Info($"Begin local file system scan");


            var res1 = _fileManagerService.ScanLocalFileSystem();
            _logCtrl.Info($"Local file system was sacaned. The total number of local releases: {res1.Count}");
            if (res1.Count > Consts.MAX_NUMBER_LOCAL_RELEASES)
            {
                _logCtrl.Info($"Too many local files. Cleaning ...");
                res1 = _fileManagerService.DeleteOldRelease(res1, Consts.MAX_NUMBER_LOCAL_RELEASES);
            }

            await _mediator.Publish(new LocalFileSystemWasSacanedEvent(res1), cancellationToken);
            _logCtrl.Info($"Begin donload release info form: {_registry.ReleaseInfoUrl}");
            await _releaseInfoService.FetchReleaseInfoAsync();
            var res2 = _releaseInfoService.GetCurrentReleaseInfo();
            await _mediator.Publish(new ReleaseInfoWasFetchedEvent(res2), cancellationToken);
            _logCtrl.Info($"Release info was fetched. The total number releases on server: {res2.Count}");
            await
                _mediator.Publish(new LongProcessEndedEvent("Release info was fetched", int.MinValue,
                    LongProcessType.IsIndeterminate), cancellationToken);
        }
    }
}