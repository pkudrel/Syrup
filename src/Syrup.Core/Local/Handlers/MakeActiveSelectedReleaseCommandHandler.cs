using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NLog;
using Optional.Unsafe;
using Syrup.Common;
using Syrup.Common.Io;
using Syrup.Core.Global.Events;
using Syrup.Core.Local.Commands;
using Syrup.Core.Local.Events;
using Syrup.Core.Local.Services;
using Syrup.Core.ScriptExecutor;
using Syrup.Core.Server.Logic;
using Syrup.Core._Infrastructure.Global;

namespace Syrup.Core.Local.Handlers
{
    public class MakeActiveSelectedReleaseCommandHandler : INotificationHandler<MakeActiveSelectedReleaseCommand>
    {
        private static readonly Logger _logCtrl = LogManager.GetLogger(Consts.LOG_UI);
        private readonly IFileDownloader _fileDownloader;
        private readonly IFileManagerService _fileManagerService;
        private readonly ILocalReleaseService _localReleaseService;
        private readonly IMediator _mediator;
        private readonly Registry _registry;
        private readonly IScriptService _scriptService;


        public MakeActiveSelectedReleaseCommandHandler(
            IMediator mediator,
            Registry registry,
            IFileManagerService fileManagerService,
            ILocalReleaseService localReleaseService,
            IFileDownloader fileDownloader,
            IScriptService scriptService
        )
        {
            _mediator = mediator;
            _registry = registry;
            _fileManagerService = fileManagerService;
            _localReleaseService = localReleaseService;
            _fileDownloader = fileDownloader;
            _scriptService = scriptService;
        }

        public async Task Handle(MakeActiveSelectedReleaseCommand notification, CancellationToken cancellationToken)
        {
            _logCtrl.Info($"Begin activation process app: {notification.LocalReleaseInfoDto.Name}");

            await _mediator
                .Publish(new LongProcessStartedEvent($"Make active: {notification.LocalReleaseInfoDto.Name}",
                    int.MinValue,
                    LongProcessType.IsIndeterminate));
            await Task.Delay(500);

            var releaseOption = _localReleaseService.TryGetReleaseByName(notification.LocalReleaseInfoDto.Name);
            if (releaseOption.HasValue)
            {
                var release = releaseOption.ValueOrFailure();
                if (!release.IsLocalNuget && release.IsOnServer)
                {
                    _logCtrl.Info($"Begin donload file: {release.File}");
                    Misc.RemoveFilesInDir(_registry.SyrupTmpDirPath);
                    var p = Path.Combine(_registry.SyrupTmpDirPath, release.File);
                    await _fileDownloader.DownloadFile(release.FileUrl, p, Show);
                    _fileManagerService.PutReleaseToNugetDir(release, p);
                    _logCtrl.Info($"End donload file: {release.File}");
                }

                _logCtrl.Info($"Begin install file: {release.File}");
                _fileManagerService.ExtractNugetToApps(release);
                _logCtrl.Info($"File was instaled ");

                // run befor
                var r1 = _scriptService.RunBeforeMakeCurrent(release);
                if (!r1.Continue)
                {
                    MakeAbort(notification, r1);
                    return;
                }

                _logCtrl.Info($"Begin program activation process.");
                _fileManagerService.CopyToActiveDir(release);

                // run after
                var r2 = _scriptService.RunAfterMakeCurrent(release);
                if (!r2.Continue)
                {
                    MakeAbort(notification, r2);
                    return;
                }

                _logCtrl.Info($"End activation proces.");

                _localReleaseService.MakeActive(release);
            }

            await
                _mediator.Publish(
                    new LongProcessEndedEvent($"{notification.LocalReleaseInfoDto.Name} was activated", int.MinValue,
                        LongProcessType.IsIndeterminate));

            await _mediator.Publish(new LocalReleaseInfoListWasUpdatedEvent(), cancellationToken);
        }


        private void MakeAbort(MakeActiveSelectedReleaseCommand notification, SyrupExecuteResult executeResult)
        {
            _logCtrl.Info($"Error: {executeResult.Messages.First()}");
            _mediator.Publish(
                new LongProcessEndedEvent($"{notification.LocalReleaseInfoDto.Name} activation faild", int.MinValue,
                    LongProcessType.IsIndeterminate));
        }

        public async void Show(int i)
        {
            //_logCtrl.Info($"Progres: {i}%");
            await
                _mediator.Publish(new LongProcessNotifiedEvent($"Progres: {i}%", int.MinValue,
                    LongProcessType.IsIndeterminate));
        }
    }
}