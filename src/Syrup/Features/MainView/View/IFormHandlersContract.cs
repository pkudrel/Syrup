using Syrup.Core.Global.Events;
using Syrup.Core.Local.ReqRes;
using Syrup.Core.Server.Events;
using Syrup.Core.Server.ReqRes;

namespace Syrup.Features.MainView.View
{
    public interface IFormHandlersContract
    {
        void Execute(LongProcessStartedEvent longProcessStartedEvent);
        void Execute(LongProcessNotifiedEvent longProcessNotifiedEvent);
        void Execute(LongProcessEndedEvent longProcessEndedEvent);
        void Execute(ReleaseInfoWasFetchedEvent releaseInfoWasFetchedEvent);
        void Execute(MakeUpdatePanelVisibleRequest makeUpdatePanelVisibleRequest);
        void Execute(GetLocalReleaseListQueryResult getLocalReleaseListQueryResult);
    }
}