using MediatR;

namespace InventoryService.shared.MarkerInterface
{
    public interface ICommand<out TResponse> : IRequest<TResponse> { }
}
