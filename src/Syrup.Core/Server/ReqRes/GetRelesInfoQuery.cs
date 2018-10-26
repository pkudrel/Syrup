using System.Collections.Generic;
using MediatR;
using Optional;
using Syrup.Core.Server.Models;

namespace Syrup.Core.Server.ReqRes
{
    public class GetRelaseInfoQuery : IRequest<Option<GetRelaseInfo>>
    {
    }

    public class GetRelaseInfo
    {
        public GetRelaseInfo(List<ReleaseInfo> relaseInfos)
        {
            RelaseInfos = relaseInfos;
        }

        public List<ReleaseInfo> RelaseInfos { get; }
    }
}