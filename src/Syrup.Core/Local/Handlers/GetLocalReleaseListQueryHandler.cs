using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Optional;
using Syrup.Core.Local.Dto;
using Syrup.Core.Local.ReqRes;
using Syrup.Core.Local.Services;

namespace Syrup.Core.Local.Handlers
{
    public class GetLocalReleaseListQueryHandler :
        IRequestHandler<GetLocalReleaseListQuery, Option<GetLocalReleaseListQueryResult>>
    {
        private readonly ILocalReleaseService _localReleaseService;
        private readonly IMapper _mapper;

        public GetLocalReleaseListQueryHandler(IMapper mapper, ILocalReleaseService localReleaseService)
        {
            _mapper = mapper;
            _localReleaseService = localReleaseService;
        }


    
        public Task<Option<GetLocalReleaseListQueryResult>> Handle(GetLocalReleaseListQuery request, CancellationToken cancellationToken)
        {
            var list = _localReleaseService.GetLocalReleaseList();
            var localReleaseInfoDto = _mapper.Map<List<LocalReleaseInfoDto>>(list);


            return Task.FromResult(Option.Some(new GetLocalReleaseListQueryResult(localReleaseInfoDto)));
        }
    }
}