using MediatR;

namespace Syrup.Core.Local.Commands
{
    public class MakeUpdateRequest : IRequest<MakeUpdateResponse>
    {
        

        public MakeUpdateRequest()
        {
            

        }
    }

    public class MakeUpdateResponse
    {
    }
}