using MediatR;
using OrderService.Data;
using OrderService.Models;
using OrderService.Models.enums;
using OrderService.Services.Cart;
using OrderService.Services.DTOs;
using OrderService.Services.Inventory;
using OrderService.Services.Payment;
using OrderService.Services.UserProfile;

namespace OrderService.Features.Commands.PlaceOrder
{
    public record PlaceOrderCommand(
        Guid UserId,
        Guid DeliveryAddressId,
        PaymentMethod PaymentMethod,
        Guid? CartId = null,
        string? Notes = null) : IRequest<PlaceOrderResultDto>;

    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResultDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartServiceClient _cartServiceClient;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IProfileServiceClient _profileServiceClient;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PlaceOrderCommandHandler> _logger;

        public PlaceOrderCommandHandler(
            IOrderRepository orderRepository,
            ICartServiceClient cartServiceClient,
            IInventoryServiceClient inventoryServiceClient,
            IProfileServiceClient profileServiceClient,
            IPaymentService paymentService,
            ILogger<PlaceOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _cartServiceClient = cartServiceClient;
            _inventoryServiceClient = inventoryServiceClient;
            _profileServiceClient = profileServiceClient;
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task<PlaceOrderResultDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Get cart items
                var cartItems = await GetCartItemsAsync(request.UserId, request.CartId, cancellationToken);
                if (!cartItems.Any())
                    throw new InvalidOperationException("Cart is empty");

                // 2. Validate inventory for all items
                await ValidateInventoryAsync(cartItems, cancellationToken);

                // 3. Get delivery address
                var address = await GetDeliveryAddressAsync(request.UserId, request.DeliveryAddressId, cancellationToken);
                if (address == null)
                    throw new ArgumentException("Invalid delivery address");

                // 4. Calculate totals
                var totals = CalculateTotals(cartItems);

                // 5. Create order
                var order = Order.Create(
                    userId: request.UserId,
                    orderNumber: Order.GenerateOrderNumber(),
                    paymentMethod: request.PaymentMethod,
                    subTotal: totals.SubTotal,
                    deliveryFee: totals.DeliveryFee,
                    discount: totals.Discount,
                    tax: totals.Tax,
                    total: totals.Total,
                    deliveryAddressId: request.DeliveryAddressId,
                    deliveryAddressJson: address.ToJson(),
                    cartId: request.CartId,
                    notes: request.Notes
                );

                // 6. Add items to order
                foreach (var item in cartItems)
                {
                    order.AddItem(
                        productId: item.ProductId,
                        productName: item.Name,
                        unitPrice: item.UnitPrice,
                        quantity: item.Quantity,
                        imageUrl: item.ImageUrl,
                        discount: item.Discount
                    );
                }

                // 7. Handle payment
                Payment? payment = null;
                if (request.PaymentMethod == PaymentMethod.CreditCard)
                {
                    // Create payment intent with Stripe
                    var userProfile = await _profileServiceClient.GetUserProfileAsync(request.UserId, cancellationToken);
                    var paymentIntent = await _paymentService.CreatePaymentIntentAsync(
                        amount: totals.Total,
                        currency: "usd",
                        customerEmail: userProfile?.Email ?? "customer@example.com",
                        orderNumber: order.OrderNumber,
                        cancellationToken
                    );

                    payment = Payment.Create(
                        orderId: 0, // Will be set after order is saved
                        method: PaymentMethod.CreditCard,
                        amount: totals.Total
                    );

                    payment.MarkAsProcessing(paymentIntent.Id);
                    order.SetPayment(payment);
                }
                else
                {
                    // Cash on delivery
                    payment = Payment.Create(
                        orderId: 0,
                        method: PaymentMethod.CashOnDelivery,
                        amount: totals.Total
                    );

                    payment.MarkAsCompleted($"COD-{Guid.NewGuid()}");
                    order.SetPayment(payment);
                    order.UpdatePaymentStatus(PaymentStatus.Completed);
                }

                // 8. Create delivery tracking
                var delivery = Delivery.Create(orderId: 0); // Will be set after order is saved
                order.SetDelivery(delivery);

                // 9. Save order
                await _orderRepository.AddAsync(order, cancellationToken);
                await _orderRepository.SaveChangesAsync(cancellationToken);

                // 10. Clear cart if it exists
                if (request.CartId.HasValue)
                {
                    await _cartServiceClient.ClearCartAsync(request.CartId.Value, cancellationToken);
                }

                // 11. Log success
                _logger.LogInformation("Order placed successfully. OrderNumber: {OrderNumber}, UserId: {UserId}",
                    order.OrderNumber, request.UserId);

                // 12. Return result
                return MapToResultDto(order, payment!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to place order for user {UserId}", request.UserId);
                throw;
            }
        }

        private async Task<List<CartItemDto>> GetCartItemsAsync(Guid userId, Guid? cartId, CancellationToken cancellationToken)
        {
            if (cartId.HasValue)
            {
                var cart = await _cartServiceClient.GetCartByIdAsync(cartId.Value, cancellationToken);
                return cart?.Items?.ToList() ?? new List<CartItemDto>();
            }
            else
            {
                var cart = await _cartServiceClient.GetActiveCartByUserIdAsync(userId, cancellationToken);
                return cart?.Items?.ToList() ?? new List<CartItemDto>();
            }
        }

        private async Task ValidateInventoryAsync(List<CartItemDto> cartItems, CancellationToken cancellationToken)
        {
            foreach (var item in cartItems)
            {
                var isAvailable = await _inventoryServiceClient.ValidateProductAvailabilityAsync(
                    item.ProductId,
                    item.Quantity,
                    cancellationToken
                );

                if (!isAvailable)
                    throw new InvalidOperationException($"Product {item.ProductId} is not available in requested quantity");
            }
        }

        private async Task<AddressDto?> GetDeliveryAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken)
        {
            var addresses = await _profileServiceClient.GetUserAddressesAsync(userId, cancellationToken);
            return addresses?.FirstOrDefault(a => a.Id == addressId);
        }

        private (decimal SubTotal, decimal DeliveryFee, decimal Discount, decimal Tax, decimal Total)
            CalculateTotals(List<CartItemDto> cartItems)
        {
            var subTotal = cartItems.Sum(i => i.TotalPrice);
            var deliveryFee = CalculateDeliveryFee(subTotal);
            var discount = 0m; // TODO: Apply offers/promotions
            var tax = CalculateTax(subTotal - discount);
            var total = subTotal - discount + tax + deliveryFee;

            return (subTotal, deliveryFee, discount, tax, total);
        }

        private decimal CalculateDeliveryFee(decimal subTotal)
        {
            // Free delivery for orders over $1000
            return subTotal > 1000 ? 0 : 50;
        }

        private decimal CalculateTax(decimal amount)
        {
            // Assuming 10% tax
            return amount * 0.10m;
        }

        private PlaceOrderResultDto MapToResultDto(Order order, Payment payment)
        {
            return new PlaceOrderResultDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                PaymentStatus = payment.Status,
                PaymentMethod = order.PaymentMethod,
                SubTotal = order.SubTotal,
                DeliveryFee = order.DeliveryFee,
                Discount = order.Discount,
                Tax = order.Tax,
                Total = order.Total,
                DeliveryAddressId = order.DeliveryAddressId,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new OrderItemResultDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };
        }
    }

}
