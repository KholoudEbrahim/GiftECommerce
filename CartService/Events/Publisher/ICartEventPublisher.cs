using CartService.Models;

namespace CartService.Events.Publisher
{
    public interface ICartEventPublisher
    {
        Task PublishCartUpdatedAsync(Cart cart, CancellationToken cancellationToken = default);
        Task PublishCartCheckedOutAsync(Cart cart, int orderId, string orderNumber, CancellationToken cancellationToken = default);
    }
}
