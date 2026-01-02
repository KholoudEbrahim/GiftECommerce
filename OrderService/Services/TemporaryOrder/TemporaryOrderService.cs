using OrderService.Events;
using OrderService.Events.Publisher;
using OrderService.Services.DTOs;

namespace OrderService.Services.TemporaryOrder
{
    public class TemporaryOrderService : ITemporaryOrderService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<TemporaryOrderService> _logger;

        public TemporaryOrderService(
            IEventPublisher eventPublisher,
            ILogger<TemporaryOrderService> logger)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<InventoryLockResponseEvent?> RequestInventoryLockBeforeOrderCreation(
            List<CartItemDto> cartItems,
            CancellationToken cancellationToken = default)
        {
            var tempOrderId = 0;
            var tempOrderNumber = $"TEMP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";

            _logger.LogInformation("Requesting inventory lock for temporary order {TempOrderNumber}",
                tempOrderNumber);

            var inventoryItems = cartItems.Select(i => new InventoryItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList();

            return await _eventPublisher.RequestInventoryLockAsync(
                tempOrderId,
                tempOrderNumber,
                inventoryItems,
                cancellationToken);
        }

        public async Task RollbackInventoryLockOnOrderFailure(
            int orderId,
            string orderNumber,
            List<CartItemDto> cartItems,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var items = cartItems.Select(i => new InventoryItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList();

                await _eventPublisher.PublishInventoryRollbackAsync(
                    orderId,
                    orderNumber,
                    items,
                    cancellationToken);

                _logger.LogWarning(
                    "Rolled back inventory lock for failed order {OrderNumber}",
                    orderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rollback inventory lock for order {OrderNumber}",
                    orderNumber);
            }
        }
    }
}