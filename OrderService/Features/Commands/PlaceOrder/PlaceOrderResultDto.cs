using OrderService.Models.enums;

namespace OrderService.Features.Commands.PlaceOrder
{
    public record PlaceOrderResultDto
    {
        public int OrderId { get; init; }
        public string OrderNumber { get; init; } = default!;
        public OrderStatus Status { get; init; }
        public PaymentStatus PaymentStatus { get; init; }
        public PaymentMethod PaymentMethod { get; init; }
        public decimal SubTotal { get; init; }
        public decimal DeliveryFee { get; init; }
        public decimal Discount { get; init; }
        public decimal Tax { get; init; }
        public decimal Total { get; init; }
        public Guid DeliveryAddressId { get; init; }
        public DateTime CreatedAt { get; init; }
        public List<OrderItemResultDto> Items { get; init; } = new();
        public string? PaymentInstructions { get; init; }
    }

}
