using MediatR;
using OrderService.Data;
using OrderService.Events;
using OrderService.Events.Publisher;
using OrderService.Features.Commands.PlaceOrder;
using OrderService.Models;
using OrderService.Models.enums;
using OrderService.Services.Cart;
using OrderService.Services.DTOs;
using OrderService.Services.Inventory;
using OrderService.Services.Payment;
using OrderService.Services.TemporaryOrder;
using OrderService.Services.UserProfile;

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
    private readonly ITemporaryOrderService _temporaryOrderService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<PlaceOrderCommandHandler> _logger;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        ICartServiceClient cartServiceClient,
        IInventoryServiceClient inventoryServiceClient,
        IProfileServiceClient profileServiceClient,
        IPaymentService paymentService,
        ITemporaryOrderService temporaryOrderService,
        IEventPublisher eventPublisher,
        ILogger<PlaceOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _cartServiceClient = cartServiceClient;
        _inventoryServiceClient = inventoryServiceClient;
        _profileServiceClient = profileServiceClient;
        _paymentService = paymentService;
        _temporaryOrderService = temporaryOrderService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<PlaceOrderResultDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        List<CartItemDto> cartItems = null!;
        Order? order = null;

        try
        {
            _logger.LogInformation("Starting order placement for user {UserId}", request.UserId);

    
            cartItems = await GetCartItemsAsync(request.UserId, request.CartId, cancellationToken);
            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            _logger.LogDebug("Retrieved {Count} cart items", cartItems.Count);

   
            var inventoryLockResponse = await _temporaryOrderService
                .RequestInventoryLockBeforeOrderCreation(cartItems, cancellationToken);

            if (inventoryLockResponse == null || !inventoryLockResponse.Success)
            {
                var errorMessage = inventoryLockResponse?.FailureReason ?? "Inventory lock failed";
                var unavailableItems = inventoryLockResponse?.UnavailableItems;

                throw new InvalidOperationException(
                    unavailableItems != null
                        ? $"Products unavailable: {string.Join(", ", unavailableItems
                            .Select(i => $"Product {i.ProductId}: requested {i.RequestedQuantity}, available {i.AvailableQuantity}"))}"
                        : errorMessage
                );
            }

            _logger.LogInformation("Inventory lock successful");

       
            var address = await GetDeliveryAddressAsync(request.UserId, request.DeliveryAddressId, cancellationToken);
            if (address == null)
                throw new ArgumentException("Invalid delivery address");

   
            var totals = CalculateTotals(cartItems);

 
            order = Order.Create(
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

      
            await _orderRepository.AddAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order created: ID={OrderId}, Number={OrderNumber}",
                order.Id, order.OrderNumber);

   
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

 
            OrderService.Models.Payment? payment = null;
            if (request.PaymentMethod == OrderService.Models.enums.PaymentMethod.CreditCard)
            {
                payment = await ProcessCreditCardPaymentAsync(order, totals.Total, cancellationToken);
            }
            else if (request.PaymentMethod == OrderService.Models.enums.PaymentMethod.CashOnDelivery)
            {
                payment = ProcessCashOnDeliveryPayment(order, totals.Total);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported payment method: {request.PaymentMethod}");
            }


            var delivery = Delivery.Create(orderId: order.Id);
            order.SetDelivery(delivery);


            await _orderRepository.SaveChangesAsync(cancellationToken);


            await _eventPublisher.PublishOrderPlacedAsync(order, cancellationToken);


            if (request.CartId.HasValue)
            {
                await ClearCartAsync(request.CartId.Value, cancellationToken);
            }

            _logger.LogInformation("Order {OrderNumber} placed successfully", order.OrderNumber);


            return MapToResultDto(order, payment);
        }
        catch (Exception ex)
        {

            await RollbackOnFailure(order, cartItems, ex, cancellationToken);
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

    private async Task<AddressDto?> GetDeliveryAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken)
    {
        var addresses = await _profileServiceClient.GetUserAddressesAsync(userId, cancellationToken);
        return addresses?.FirstOrDefault(a => a.Id == addressId);
    }

    private (decimal SubTotal, decimal DeliveryFee, decimal Discount, decimal Tax, decimal Total)
        CalculateTotals(List<CartItemDto> cartItems)
    {
        var subTotal = cartItems.Sum(i => i.TotalPrice);
        var deliveryFee = subTotal > 1000 ? 0 : 50;
        var discount = 0m;
        var tax = (subTotal - discount) * 0.10m;
        var total = subTotal - discount + tax + deliveryFee;

        return (subTotal, deliveryFee, discount, tax, total);
    }

    private async Task<OrderService.Models.Payment> ProcessCreditCardPaymentAsync(
            OrderService.Models.Order order,
            decimal amount,
            CancellationToken cancellationToken)
    {
        var userProfile = await _profileServiceClient.GetUserProfileAsync(order.UserId, cancellationToken);

        var paymentIntent = await _paymentService.CreatePaymentIntentAsync(
            amount: amount,
            currency: "usd",
            customerEmail: userProfile?.Email ?? "customer@example.com",
            orderNumber: order.OrderNumber,
            cancellationToken
        );

        var payment = OrderService.Models.Payment.Create(
            orderId: order.Id,
            method: OrderService.Models.enums.PaymentMethod.CreditCard,
            amount: amount
        );

        payment.MarkAsProcessing(paymentIntent.Id);
        order.SetPayment(payment);

        _logger.LogInformation("Created credit card payment intent: {PaymentIntentId}",
            paymentIntent.Id);

        return payment;
    }
    private async Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken)
    {
        try
        {
            await _cartServiceClient.ClearCartAsync(cartId, cancellationToken);
            _logger.LogDebug("Cleared cart {CartId}", cartId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clear cart {CartId}", cartId);
           
        }
    }
    private async Task RollbackOnFailure(
          OrderService.Models.Order? order,
          List<CartItemDto>? cartItems,
          Exception exception,
          CancellationToken cancellationToken)
    {
        if (order != null && cartItems != null && cartItems.Any())
        {
            try
            {
                await _temporaryOrderService.RollbackInventoryLockOnOrderFailure(
                    order.Id,
                    order.OrderNumber,
                    cartItems,
                    cancellationToken);

                _logger.LogWarning(exception,
                    "Rolled back inventory lock for failed order {OrderNumber}",
                    order.OrderNumber);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx,
                    "Failed to rollback inventory lock for order {OrderNumber}",
                    order.OrderNumber);
            }
        }

        _logger.LogError(exception, "Order placement failed");
    }
    private OrderService.Models.Payment ProcessCashOnDeliveryPayment(
        OrderService.Models.Order order,
        decimal amount)
    {
        var payment = OrderService.Models.Payment.Create(
            orderId: order.Id,
            method: OrderService.Models.enums.PaymentMethod.CashOnDelivery,
            amount: amount
        );

        payment.MarkAsAwaitingCashPayment();
        order.SetPayment(payment);
        order.UpdatePaymentStatus(OrderService.Models.enums.PaymentStatus.AwaitingCashPayment);

        _logger.LogInformation("Created cash on delivery payment for order {OrderNumber}",
            order.OrderNumber);

        return payment;
    }

    private PlaceOrderResultDto MapToResultDto(
             OrderService.Models.Order order,
             OrderService.Models.Payment payment)
    {
     
        var result = new PlaceOrderResultDto
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
            }).ToList(),
           
            PaymentInstructions = order.PaymentMethod == OrderService.Models.enums.PaymentMethod.CashOnDelivery
                ? "Please prepare exact cash amount for delivery. Payment will be verified upon delivery."
                : null
        };

        return result;
    }

}




