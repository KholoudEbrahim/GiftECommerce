using OrderService.Events;
using OrderService.Models;
using OrderService.Services.DTOs;

namespace OrderService.Events.Publisher
{
    public interface IEventPublisher
    {
        Task PublishOrderPlacedAsync(Order order, CancellationToken cancellationToken = default);
        Task PublishOrderStatusUpdatedAsync(Order order, string oldStatus, CancellationToken cancellationToken = default);
        Task PublishPaymentCompletedAsync(Order order, Payment payment, CancellationToken cancellationToken = default);
        Task<InventoryLockResponseEvent?> RequestInventoryLockAsync(
            int orderId,
            string orderNumber,
            List<InventoryItem> items,
            CancellationToken cancellationToken = default);
        Task PublishInventoryRollbackAsync(
            int orderId,
            string orderNumber,
            List<InventoryItem> items,
            CancellationToken cancellationToken = default);
        Task PublishRefundCompletedAsync(
    Order order,
    Payment payment,
    string refundId,
    decimal refundAmount,
    string reason,
    CancellationToken cancellationToken = default);
    }
}
