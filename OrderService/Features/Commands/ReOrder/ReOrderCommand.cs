using MediatR;
using OrderService.Data;
using OrderService.Features.Endpoints.DTOs;
using OrderService.Models;
using OrderService.Models.enums;
using OrderService.Services.DTOs;
using OrderService.Services.Inventory;
using OrderService.Services.UserProfile;

namespace OrderService.Features.Commands.ReOrder
{
    public record ReOrderCommand(
      Guid UserId,
      string OrderNumber,
      Guid? NewAddressId = null,
      List<ModifiedItemDto>? ModifiedItems = null) : IRequest<ReOrderResultDto>;

    public class ReOrderCommandHandler : IRequestHandler<ReOrderCommand, ReOrderResultDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProfileServiceClient _profileServiceClient;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly ILogger<ReOrderCommandHandler> _logger;

        public ReOrderCommandHandler(
            IOrderRepository orderRepository,
            IProfileServiceClient profileServiceClient,
            IInventoryServiceClient inventoryServiceClient,
            ILogger<ReOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _profileServiceClient = profileServiceClient;
            _inventoryServiceClient = inventoryServiceClient;
            _logger = logger;
        }

        public async Task<ReOrderResultDto> Handle(ReOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Get the original order
                var originalOrder = await _orderRepository.GetOrderWithDetailsAsync(request.OrderNumber, cancellationToken);
                if (originalOrder == null || originalOrder.UserId != request.UserId)
                    throw new KeyNotFoundException($"Order {request.OrderNumber} not found");

                // 2. Validate that the original order was delivered
                if (originalOrder.Status != OrderStatus.Delivered)
                    throw new InvalidOperationException("Only delivered orders can be reordered");

                // 3. Get delivery address
                AddressDto? address = null;
                if (request.NewAddressId.HasValue)
                {
                    address = await _profileServiceClient.GetAddressByIdAsync(request.NewAddressId.Value, cancellationToken);
                }
                else
                {
                    var addresses = await _profileServiceClient.GetUserAddressesAsync(request.UserId, cancellationToken);
                    address = addresses?.FirstOrDefault(a => a.Id == originalOrder.DeliveryAddressId);
                }

                if (address == null)
                    throw new ArgumentException("Delivery address not found");

                // 4. Prepare items for new order
                var newOrderItems = new List<CartItemDto>();
                foreach (var originalItem in originalOrder.Items)
                {
                    // Check if item is modified or removed
                    var modifiedItem = request.ModifiedItems?.FirstOrDefault(m => m.ProductId == originalItem.ProductId);

                    if (modifiedItem?.Remove == true)
                    {
                        // Skip this item (removed by user)
                        continue;
                    }

                    var newQuantity = modifiedItem?.NewQuantity ?? originalItem.Quantity;

                    // Validate inventory for each item
                    var isAvailable = await _inventoryServiceClient.ValidateProductAvailabilityAsync(
                        originalItem.ProductId,
                        newQuantity,
                        cancellationToken
                    );

                    if (!isAvailable)
                        throw new InvalidOperationException($"Product {originalItem.ProductId} is not available in requested quantity");

                    newOrderItems.Add(new CartItemDto
                    {
                        ProductId = originalItem.ProductId,
                        Name = originalItem.ProductName,
                        UnitPrice = originalItem.UnitPrice,
                        Quantity = newQuantity,
                        ImageUrl = originalItem.ImageUrl,
                
                    });
                }

                // 5. Check if any items remain
                if (!newOrderItems.Any())
                    throw new InvalidOperationException("Cannot create empty order");

                // 6. Calculate totals
                var subTotal = newOrderItems.Sum(i => i.TotalPrice);
                var deliveryFee = CalculateDeliveryFee(subTotal);
                var tax = CalculateTax(subTotal);
                var total = subTotal + deliveryFee + tax;

                // 7. Create new order
                var newOrder = Order.Create(
                    userId: request.UserId,
                    orderNumber: Order.GenerateOrderNumber(),
                    paymentMethod: originalOrder.PaymentMethod,
                    subTotal: subTotal,
                    deliveryFee: deliveryFee,
                    discount: 0,
                    tax: tax,
                    total: total,
                    deliveryAddressId: address.Id,
                    deliveryAddressJson: address.ToJson(),
                    cartId: null,
                    notes: $"Reordered from order {originalOrder.OrderNumber}"
                );

                // 8. Add items to new order
                foreach (var item in newOrderItems)
                {
                    newOrder.AddItem(
                        productId: item.ProductId,
                        productName: item.Name,
                        unitPrice: item.UnitPrice,
                        quantity: item.Quantity,
                        imageUrl: item.ImageUrl
                    );
                }

                // 9. Save new order
                await _orderRepository.AddAsync(newOrder, cancellationToken);
                await _orderRepository.SaveChangesWithIncludesAsync(cancellationToken);

                // 10. Log success
                _logger.LogInformation("Reordered successfully. Original: {OriginalOrder}, New: {NewOrder}",
                    originalOrder.OrderNumber, newOrder.OrderNumber);

                // 11. Return result
                return MapToResultDto(newOrder, originalOrder.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reorder for user {UserId}", request.UserId);
                throw;
            }
        }

        private decimal CalculateDeliveryFee(decimal subTotal)
        {
            return subTotal > 1000 ? 0 : 50;
        }

        private decimal CalculateTax(decimal amount)
        {
            return amount * 0.10m;
        }

        private ReOrderResultDto MapToResultDto(Order newOrder, string originalOrderNumber)
        {
            return new ReOrderResultDto
            {
                NewOrderId = newOrder.Id,
                NewOrderNumber = newOrder.OrderNumber,
                OriginalOrderNumber = originalOrderNumber,
                Status = newOrder.Status,
                SubTotal = newOrder.SubTotal,
                DeliveryFee = newOrder.DeliveryFee,
                Tax = newOrder.Tax,
                Total = newOrder.Total,
                ItemsCount = newOrder.Items.Count,
                CreatedAt = newOrder.CreatedAt,
                Items = newOrder.Items.Select(i => new ReOrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };
        }
    }

}
