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
    string? Notes = null
) : IRequest<PlaceOrderResultDto>;

public class PlaceOrderCommandHandler
    : IRequestHandler<PlaceOrderCommand, PlaceOrderResultDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartServiceClient _cartServiceClient;
    private readonly IProfileServiceClient _profileServiceClient;
    private readonly IPaymentService _paymentService;
    private readonly ITemporaryOrderService _temporaryOrderService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<PlaceOrderCommandHandler> _logger;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        ICartServiceClient cartServiceClient,
        IProfileServiceClient profileServiceClient,
        IPaymentService paymentService,
        ITemporaryOrderService temporaryOrderService,
        IEventPublisher eventPublisher,
        ILogger<PlaceOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _cartServiceClient = cartServiceClient;
        _profileServiceClient = profileServiceClient;
        _paymentService = paymentService;
        _temporaryOrderService = temporaryOrderService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<PlaceOrderResultDto> Handle(
        PlaceOrderCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting order placement for user {UserId}",
            request.UserId);


        var cart = await _cartServiceClient.GetActiveCartByUserIdAsync(
            request.UserId,
            cancellationToken);

        if (cart == null || cart.Items == null || !cart.Items.Any())
            throw new InvalidOperationException(
                "No active cart found for user or cart is empty.");

        var cartItems = cart.Items.ToList();


        var address = await GetDeliveryAddressAsync(
            request.UserId,
            request.DeliveryAddressId,
            cancellationToken);

        if (address == null)
            throw new ArgumentException(
                $"Delivery address {request.DeliveryAddressId} not found or does not belong to user");


        var inventoryResult =
            await _temporaryOrderService.RequestInventoryLockBeforeOrderCreation(
                cartItems,
                cancellationToken);

        if (inventoryResult == null || !inventoryResult.Success)
            throw new InvalidOperationException(
                "Some items are not available or inventory service is unavailable.");


        var totals = CalculateTotals(cartItems);

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
            cartId: null,
            notes: request.Notes
        );

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


        try
        {
            if (request.PaymentMethod == PaymentMethod.CreditCard)
            {
                await CreateCreditCardPaymentAsync(
                    order,
                    totals.Total,
                    cancellationToken);
            }
            else
            {
                CreateCashOnDeliveryPayment(order, totals.Total);
            }
        }
        catch
        {
            await RollbackInventory(order, cartItems, cancellationToken);
            throw;
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        await _cartServiceClient.ClearCartAsync(cancellationToken);

        await _eventPublisher.PublishOrderPlacedAsync(order, cancellationToken);

        return MapToResult(order);
    }

    private async Task<AddressDto?> GetDeliveryAddressAsync(
        Guid userId,
        Guid addressId,
        CancellationToken ct)
    {
        var addresses = await _profileServiceClient
            .GetUserAddressesAsync(userId, ct);

        return addresses?.FirstOrDefault(a => a.Id == addressId);
    }

    private static (decimal SubTotal, decimal DeliveryFee, decimal Discount, decimal Tax, decimal Total)
        CalculateTotals(List<CartItemDto> items)
    {
        var subTotal = items.Sum(i => i.TotalPrice);
        var deliveryFee = subTotal > 1000 ? 0 : 50;
        var discount = 0m;
        var tax = (subTotal - discount) * 0.10m;
        var total = subTotal + tax + deliveryFee - discount;

        return (subTotal, deliveryFee, discount, tax, total);
    }

    private async Task CreateCreditCardPaymentAsync(
        Order order,
        decimal amount,
        CancellationToken ct)
    {
        var paymentIntent =
            await _paymentService.CreatePaymentIntentAsync(
                amount,
                "egp",
                order.UserId.ToString(),
                order.OrderNumber,
                ct);

        var payment = Payment.Create(
            orderId: order.Id,
            method: PaymentMethod.CreditCard,
            amount: amount);

        payment.MarkAsProcessing(paymentIntent.Id);
        order.SetPayment(payment);
    }

    private static void CreateCashOnDeliveryPayment(
        Order order,
        decimal amount)
    {
        var payment = Payment.Create(
            orderId: order.Id,
            method: PaymentMethod.CashOnDelivery,
            amount: amount);

        payment.MarkAsAwaitingCashPayment();
        order.SetPayment(payment);
    }

    private async Task RollbackInventory(
        Order order,
        List<CartItemDto> items,
        CancellationToken ct)
    {
        await _temporaryOrderService
            .RollbackInventoryLockOnOrderFailure(
                order.Id,
                order.OrderNumber,
                items,
                ct);
    }

    private static PlaceOrderResultDto MapToResult(Order order)
    {
        return new PlaceOrderResultDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            PaymentStatus = order.Payment!.Status,
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
