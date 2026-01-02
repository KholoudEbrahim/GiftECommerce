using MassTransit;
using OrderService.Events;
using OrderService.Models;
using OrderService.Services.DTOs;

namespace OrderService.Events.Publisher
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRequestClient<InventoryLockRequestEvent> _inventoryRequestClient;
        private readonly ILogger<EventPublisher> _logger;

        public EventPublisher(
            IPublishEndpoint publishEndpoint,
            IRequestClient<InventoryLockRequestEvent> inventoryRequestClient,
            ILogger<EventPublisher> logger)
        {
            _publishEndpoint = publishEndpoint;
            _inventoryRequestClient = inventoryRequestClient;
            _logger = logger;
        }

        public async Task PublishOrderPlacedAsync(Order order, CancellationToken cancellationToken = default)
        {
            var orderPlacedEvent = new OrderPlacedEvent
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                TotalAmount = order.Total,
                PaymentMethod = order.PaymentMethod.ToString(),
                PlacedAt = DateTime.UtcNow,
                Items = order.Items.Select(i => new OrderItemEvent
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await _publishEndpoint.Publish(orderPlacedEvent, cancellationToken);
            _logger.LogInformation("Published OrderPlacedEvent for order {OrderNumber}", order.OrderNumber);
        }

        public async Task PublishOrderStatusUpdatedAsync(Order order, string oldStatus, CancellationToken cancellationToken = default)
        {
            var statusUpdatedEvent = new OrderStatusUpdatedEvent
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                OldStatus = oldStatus,
                NewStatus = order.Status.ToString(),
                UpdatedAt = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(statusUpdatedEvent, cancellationToken);
            _logger.LogInformation("Published OrderStatusUpdatedEvent for order {OrderNumber}", order.OrderNumber);
        }

        public async Task PublishPaymentCompletedAsync(Order order, Payment payment, CancellationToken cancellationToken = default)
        {
            var paymentCompletedEvent = new PaymentCompletedEvent
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                Amount = payment.Amount,
                PaymentMethod = payment.Method.ToString(),
                TransactionId = payment.TransactionId ?? "N/A",
                CompletedAt = payment.PaidAt ?? DateTime.UtcNow
            };

            await _publishEndpoint.Publish(paymentCompletedEvent, cancellationToken);
            _logger.LogInformation("Published PaymentCompletedEvent for order {OrderNumber}", order.OrderNumber);
        }      

        public async Task PublishInventoryRollbackAsync(int orderId, string orderNumber, List<InventoryItem> items, CancellationToken cancellationToken = default)
        {
            var rollbackEvent = new InventoryRollbackEvent
            {
                OrderId = orderId,
                OrderNumber = orderNumber,
                Items = items,
                RollbackAt = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(rollbackEvent, cancellationToken);
            _logger.LogInformation("Published InventoryRollbackEvent for order {OrderNumber}", orderNumber);
        }
        public record InventoryRollbackEvent
        {
            public int OrderId { get; init; }
            public string OrderNumber { get; init; } = default!;
            public List<InventoryItem> Items { get; init; } = new();
            public DateTime RollbackAt { get; init; }
        }

        public async Task<InventoryLockResponseEvent?> RequestInventoryLockAsync(
               int orderId,
               string orderNumber,
               List<InventoryItem> items,
               CancellationToken cancellationToken = default)
        {
            var correlationId = NewId.NextGuid();

            var inventoryRequest = new InventoryLockRequestEvent
            {
                CorrelationId = correlationId,
                OrderId = orderId,
                OrderNumber = orderNumber,
                Items = items,
                RequestedAt = DateTime.UtcNow
            };

            try
            {
                var response = await _inventoryRequestClient.GetResponse<InventoryLockResponseEvent>(
                    inventoryRequest,
                    cancellationToken,
                    TimeSpan.FromSeconds(30));

                _logger.LogInformation("Inventory lock response received for order {OrderNumber}: {Success}",
                    orderNumber, response.Message.Success);

                return response.Message;
            }
            catch (RequestTimeoutException)
            {
                _logger.LogWarning("Inventory lock request timed out for order {OrderNumber}", orderNumber);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to request inventory lock for order {OrderNumber}", orderNumber);
                return null;
            }
        }
    }
    

}
