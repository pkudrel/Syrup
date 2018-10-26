using System.Threading.Tasks;

namespace Syrup.Common.Mediator
{
    public interface IAsyncPreRequestHandler<in TRequest>
    {
        Task Handle(TRequest request);
    }
}