namespace Syrup.Common.Mediator
{
    public interface IPreRequestHandler<in TRequest>
    {
        void Handle(TRequest request);
    }
}