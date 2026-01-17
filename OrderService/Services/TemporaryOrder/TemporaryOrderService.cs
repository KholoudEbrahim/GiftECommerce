using Microsoft.Extensions.Options;
using OrderService.Events;
using OrderService.Events.Publisher;
using OrderService.Services.DTOs;

namespace OrderService.Services.TemporaryOrder
{
    public class TemporaryOrderService : ITemporaryOrderService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<TemporaryOrderService> _logger;
        private readonly ExternalServicesSettings _settings;
        private static int _temporaryOrderCounter = 0;
        private static readonly object _lock = new object();

        public TemporaryOrderService(
            IEventPublisher eventPublisher,
            ILogger<TemporaryOrderService> logger,
            IOptions<ExternalServicesSettings> settings)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task<InventoryLockResponseEvent?> RequestInventoryLockBeforeOrderCreation(
         List<CartItemDto> cartItems,
         CancellationToken cancellationToken = default)
        {
            if (cartItems == null || !cartItems.Any())
            {
                _logger.LogWarning("Cannot request inventory lock for empty cart");
                return new InventoryLockResponseEvent
                {
                    Success = false,
                    FailureReason = "Cart is empty",
                    RespondedAt = DateTime.UtcNow
                };
            }

            var tempOrderId = GenerateTemporaryOrderId();
            var tempOrderNumber = GenerateTemporaryOrderNumber();

            _logger.LogInformation(
                "Requesting inventory lock for temporary order {TempOrderNumber} with {ItemCount} items",
                tempOrderNumber, cartItems.Count);

            using var timeoutCts = new CancellationTokenSource(
                TimeSpan.FromSeconds(_settings.InventoryLockTimeoutInSeconds));

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, timeoutCts.Token);

            try
            {
                var inventoryItems = cartItems.Select(i => new InventoryItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList();

                var response = await _eventPublisher.RequestInventoryLockAsync(
                    tempOrderId,
                    tempOrderNumber,
                    inventoryItems,
                    linkedCts.Token);

                if (response == null)
                {
                    _logger.LogWarning(
                        "Inventory lock request returned null for {TempOrderNumber}",
                        tempOrderNumber);

                    return new InventoryLockResponseEvent
                    {
                        Success = false,
                        FailureReason = "Inventory service did not respond",
                        RespondedAt = DateTime.UtcNow
                    };
                }

                if (response.Success)
                {
                    _logger.LogInformation(
                        "Inventory locked successfully for {TempOrderNumber}",
                        tempOrderNumber);
                }
                else
                {
                    _logger.LogWarning(
                        "Inventory lock failed for {TempOrderNumber}: {Reason}",
                        tempOrderNumber, response.FailureReason);
                }

                return response;
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)  
            {
                _logger.LogError(
                    "Inventory lock request timed out after {Timeout}s for {TempOrderNumber}",
                    _settings.InventoryLockTimeoutInSeconds, tempOrderNumber);

                return new InventoryLockResponseEvent
                {
                    Success = false,
                    FailureReason = $"Inventory service timeout after {_settings.InventoryLockTimeoutInSeconds}s",
                    RespondedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to request inventory lock for {TempOrderNumber}",
                    tempOrderNumber);

                return new InventoryLockResponseEvent
                {
                    Success = false,
                    FailureReason = "Internal error while checking inventory",
                    RespondedAt = DateTime.UtcNow
                };
            }
        }

        public async Task RollbackInventoryLockOnOrderFailure(
            int orderId,
            string orderNumber,
            List<CartItemDto> cartItems,
            CancellationToken cancellationToken = default)
        {
            if (cartItems == null || !cartItems.Any())
            {
                _logger.LogWarning(
                    "No items to rollback for order {OrderNumber}",
                    orderNumber);
                return;
            }

            try
            {
                _logger.LogWarning(
                    "Rolling back inventory lock for order {OrderNumber} with {ItemCount} items",
                    orderNumber, cartItems.Count);

                var items = cartItems.Select(i => new InventoryItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList();

   
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, timeoutCts.Token);

                await _eventPublisher.PublishInventoryRollbackAsync(
                    orderId,
                    orderNumber,
                    items,
                    linkedCts.Token);

                _logger.LogInformation(
                    "Inventory rollback published for order {OrderNumber}",
                    orderNumber);
            }
            catch (OperationCanceledException)
            {
                _logger.LogError(
                    "Inventory rollback timed out for order {OrderNumber}. Manual intervention may be required.",
                    orderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to rollback inventory for order {OrderNumber}. Manual intervention may be required.",
                    orderNumber);
            }
        }

        private static int GenerateTemporaryOrderId()
        {
            lock (_lock)
            {
                _temporaryOrderCounter++;
                if (_temporaryOrderCounter > 999999)
                {
                    _temporaryOrderCounter = 1;
                }
                return -_temporaryOrderCounter; 
            }
        }

        private static string GenerateTemporaryOrderNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpper();
            return $"TEMP-{timestamp}-{uniqueId}";
        }
    }
}