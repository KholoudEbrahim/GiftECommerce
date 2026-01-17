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
        List<ModifiedItemDto>? ModifiedItems = null
    ) : IRequest<ReOrderResultDto>;

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

        public async Task<ReOrderResultDto> Handle(
            ReOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
       
                var originalOrder = await _orderRepository.GetOrderWithDetailsAsync(
                    request.OrderNumber,
                    cancellationToken);

                if (originalOrder == null)
                    throw new KeyNotFoundException($"Order {request.OrderNumber} not found");

                if (originalOrder.UserId != request.UserId)
                    throw new UnauthorizedAccessException("You can only reorder your own orders");

                if (originalOrder.Status != OrderStatus.Delivered)
                    throw new InvalidOperationException(
                        "Only delivered orders can be reordered");

                var address = await GetDeliveryAddressAsync(
                    request.UserId,
                    request.NewAddressId,
                    originalOrder.DeliveryAddressId,
                    cancellationToken);

                if (address == null)
                    throw new ArgumentException("Delivery address not found");

           
                var newOrderItems = await PrepareOrderItemsAsync(
                    originalOrder.Items,
                    request.ModifiedItems,
                    cancellationToken);

                if (!newOrderItems.Any())
                    throw new InvalidOperationException(
                        "Cannot create empty order. All items were removed or unavailable.");

       
                var (subTotal, deliveryFee, tax, total) = CalculateTotals(newOrderItems);

 
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

    
                foreach (var item in newOrderItems)
                {
                    newOrder.AddItem(
                        productId: item.ProductId,
                        productName: item.Name,
                        unitPrice: item.UnitPrice,
                        quantity: item.Quantity,
                        imageUrl: item.ImageUrl,
                        discount: item.Discount
                    );
                }

 
                var payment = Payment.Create(
                    orderId: newOrder.Id,
                    method: originalOrder.PaymentMethod,
                    amount: total
                );

                if (originalOrder.PaymentMethod == PaymentMethod.CashOnDelivery)
                {
                    payment.MarkAsAwaitingCashPayment();
                }
                else if (originalOrder.PaymentMethod == PaymentMethod.CreditCard)
                {
             
                    payment.UpdateGatewayResponse("Awaiting payment completion");
                }

                newOrder.SetPayment(payment);

      
                await _orderRepository.AddAsync(newOrder, cancellationToken);
                await _orderRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Reorder successful. Original: {OriginalOrder}, New: {NewOrder}",
                    originalOrder.OrderNumber, newOrder.OrderNumber);

                return MapToResultDto(newOrder, originalOrder.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to reorder {OrderNumber} for user {UserId}",
                    request.OrderNumber, request.UserId);
                throw;
            }
        }

        private async Task<AddressDto?> GetDeliveryAddressAsync(
            Guid userId,
            Guid? newAddressId,
            Guid originalAddressId,
            CancellationToken ct)
        {
            if (newAddressId.HasValue)
            {
                return await _profileServiceClient.GetAddressByIdAsync(
                    newAddressId.Value,
                    ct);
            }

     
            var addresses = await _profileServiceClient.GetUserAddressesAsync(userId, ct);
            return addresses?.FirstOrDefault(a => a.Id == originalAddressId);
        }

        private async Task<List<CartItemDto>> PrepareOrderItemsAsync(
            IEnumerable<OrderItem> originalItems,
            List<ModifiedItemDto>? modifications,
            CancellationToken ct)
        {
            var newItems = new List<CartItemDto>();

            foreach (var originalItem in originalItems)
            {
              
                var modification = modifications?.FirstOrDefault(
                    m => m.ProductId == originalItem.ProductId);

                if (modification?.Remove == true)
                {
                    _logger.LogInformation(
                        "Item {ProductId} removed from reorder",
                        originalItem.ProductId);
                    continue;
                }

                var newQuantity = modification?.NewQuantity ?? originalItem.Quantity;


                var availability = await _inventoryServiceClient.CheckProductAvailabilityAsync(
                    originalItem.ProductId,
                    newQuantity,
                    ct);

                if (availability == null || !availability.IsAvailable)
                {
                    _logger.LogWarning(
                        "Product {ProductId} not available. Available: {Available}, Requested: {Requested}",
                        originalItem.ProductId,
                        availability?.AvailableQuantity ?? 0,
                        newQuantity);

                    continue;
                }

                newItems.Add(new CartItemDto(
                    productId: originalItem.ProductId,
                    name: originalItem.ProductName,
                    unitPrice: originalItem.UnitPrice,
                    quantity: newQuantity,
                    imageUrl: originalItem.ImageUrl,
                    discount: originalItem.Discount
                ));
            }

            return newItems;
        }

        private static (decimal SubTotal, decimal DeliveryFee, decimal Tax, decimal Total)
            CalculateTotals(List<CartItemDto> items)
        {
            var subTotal = items.Sum(i => i.TotalPrice);
            var deliveryFee = subTotal > 1000 ? 0 : 50;
            var tax = subTotal * 0.10m;
            var total = subTotal + deliveryFee + tax;

            return (subTotal, deliveryFee, tax, total);
        }

        private static ReOrderResultDto MapToResultDto(
            Order newOrder,
            string originalOrderNumber)
        {
            return new ReOrderResultDto
            {
                NewOrderId = newOrder.Id,
                NewOrderNumber = newOrder.OrderNumber,
                OriginalOrderNumber = originalOrderNumber,
                Status = newOrder.Status,
                PaymentMethod = newOrder.PaymentMethod,  
                PaymentStatus = newOrder.PaymentStatus,   
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