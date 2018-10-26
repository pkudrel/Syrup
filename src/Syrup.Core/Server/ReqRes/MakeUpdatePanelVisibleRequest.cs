using MediatR;

namespace Syrup.Core.Server.ReqRes
{
    public class MakeUpdatePanelVisibleRequest : IRequest<MakeUpdatePanelVisibleResponse>
    {
        public string Version { get; set; }

        public MakeUpdatePanelVisibleRequest(string version)
        {
            Version = version;
        }
    }

    public class MakeUpdatePanelVisibleResponse
    {
    }
}