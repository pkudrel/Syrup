using System.Threading.Tasks;

namespace Syrup.Common.Mediator
{
    public interface IAsyncPostRequestHandler<in TRequest, in TResponse>
    {
        Task Handle(TRequest reqeust, TResponse response);
    }
}