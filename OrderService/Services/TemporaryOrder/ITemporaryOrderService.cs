using OrderService.Events;
using OrderService.Services.DTOs;

namespace OrderService.Services.TemporaryOrder
{
    public interface ITemporaryOrderService
    {
        Task<InventoryLockResponseEvent?> RequestInventoryLockBeforeOrderCreation(
            List<CartItemDto> cartItems,
            CancellationToken cancellationToken = default);

        Task RollbackInventoryLockOnOrderFailure(
            int orderId,
            string orderNumber,
            List<CartItemDto> cartItems,
            CancellationToken cancellationToken = default);
    }

}
