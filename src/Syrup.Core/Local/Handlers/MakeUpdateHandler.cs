using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Syrup.Core.Local.Commands;
using Syrup.Core._Infrastructure.Global;

namespace Syrup.Core.Local.Handlers
{
    public class MakeUpdateHandler : IRequestHandler<MakeUpdateRequest, MakeUpdateResponse>
    {
        private readonly Registry _registry;

        public MakeUpdateHandler(Registry registry)
        {
            _registry = registry;
        }


        public Task<MakeUpdateResponse> Handle(MakeUpdateRequest request, CancellationToken cancellationToken)
        {
            Process.Start(_registry.SyrupSelfFilePath);
            return Task.FromResult(new MakeUpdateResponse());
        }
    }
}