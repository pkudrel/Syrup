using System.Collections.Generic;
using MediatR;
using Optional;
using Syrup.Core.Local.Dto;

namespace Syrup.Core.Local.ReqRes
{
    public class GetLocalReleaseListQuery : IRequest<Option<GetLocalReleaseListQueryResult>>
    {
    }

    public class GetLocalReleaseListQueryResult
    {
        public GetLocalReleaseListQueryResult(List<LocalReleaseInfoDto> localReleaseInfoDto)
        {
            LocalReleaseInfoDto = localReleaseInfoDto;
        }

        public List<LocalReleaseInfoDto> LocalReleaseInfoDto { get; }
    }
}