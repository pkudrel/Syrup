using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Syrup.Core.Local.Events;
using Syrup.Core.Local.Services;
using Syrup.Core.Server.Events;

namespace Syrup.Core.Local.Handlers
{
    public class ReleaseInfoWasFetchedEventHandler :
        INotificationHandler<ReleaseInfoWasFetchedEvent>,
        INotificationHandler<LocalFileSystemWasSacanedEvent>
    {
        private readonly ILocalReleaseService _localReleaseService;
        private readonly IMediator _mediator;

        public ReleaseInfoWasFetchedEventHandler(IMediator mediator, ILocalReleaseService localReleaseService)
        {
            _mediator = mediator;
            _localReleaseService = localReleaseService;
        }

        public async Task Handle(LocalFileSystemWasSacanedEvent notification, CancellationToken cancellationToken)
        {
          
            _localReleaseService.MakeUpdateFromLocal(notification.LocalReleaseInfos);

            await _mediator.Publish(new LocalReleaseInfoListWasUpdatedEvent(), cancellationToken);
        }

        public async Task Handle(ReleaseInfoWasFetchedEvent notification, CancellationToken cancellationToken)
        {
            _localReleaseService.MakeUpdateFromServer(notification.RelaseInfos);
            await _mediator.Publish(new LocalReleaseInfoListWasUpdatedEvent(), cancellationToken);
        }
    }
}