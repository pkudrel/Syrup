using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Syrup.Core.Global.Events;
using Syrup.Core.Local.Events;
using Syrup.Core.Local.ReqRes;
using Syrup.Core.Server.Events;
using Syrup.Core.Server.ReqRes;
using Syrup.Features.MainView.View;

namespace Syrup.Features.MainView.Handlers
{
    public class MainFormsHandlers : 
        INotificationHandler<LongProcessStartedEvent>,
        INotificationHandler<LongProcessNotifiedEvent>,
        INotificationHandler<LongProcessEndedEvent>,
        INotificationHandler<ReleaseInfoWasFetchedEvent>,
        INotificationHandler<LocalReleaseInfoListWasUpdatedEvent>,
        IRequestHandler<MakeUpdatePanelVisibleRequest, MakeUpdatePanelVisibleResponse>
    {
        private readonly IMediator _mediator;
        private readonly IFormHandlersContract _form;

        public MainFormsHandlers( IMediator mediator,  IFormHandlersContract form)
        {
            _mediator = mediator;
            _form = form;
        }

        public Task Handle(LongProcessStartedEvent notification, CancellationToken cancellationToken)
        {
            _form.Execute(notification);
            return Task.CompletedTask;
        }

        public Task Handle(LongProcessNotifiedEvent notification, CancellationToken cancellationToken)
        {
            _form.Execute(notification);
            return Task.CompletedTask;
        }

        public Task Handle(LongProcessEndedEvent notification, CancellationToken cancellationToken)
        {
            _form.Execute(notification);
            return Task.CompletedTask;
        }

        public Task Handle(ReleaseInfoWasFetchedEvent notification, CancellationToken cancellationToken)
        {
            _form.Execute(notification);
            return Task.CompletedTask;
        }

        public Task<MakeUpdatePanelVisibleResponse> Handle(MakeUpdatePanelVisibleRequest request, CancellationToken cancellationToken)
        {
            _form.Execute(request);
            return Task.FromResult(new MakeUpdatePanelVisibleResponse());
        }

        public async Task Handle(LocalReleaseInfoListWasUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var option= await _mediator.Send(new GetLocalReleaseListQuery());
            option.MatchSome(x =>
            {
                _form.Execute(x);
            });
            
           
        }
    }
}