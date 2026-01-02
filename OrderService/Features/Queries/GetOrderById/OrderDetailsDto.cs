using OrderService.Models.enums;

namespace OrderService.Features.Queries.GetOrderById
{
    public record OrderDetailsDto
    {
        public int OrderId { get; init; }
        public string OrderNumber { get; init; } = default!;
        public Guid UserId { get; init; }
        public OrderStatus Status { get; init; }
        public PaymentMethod PaymentMethod { get; init; }
        public PaymentStatus PaymentStatus { get; init; }
        public decimal SubTotal { get; init; }
        public decimal DeliveryFee { get; init; }
        public decimal Discount { get; init; }
        public decimal Tax { get; init; }
        public decimal Total { get; init; }
        public Guid DeliveryAddressId { get; init; }
        public string? Notes { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public List<OrderItemDetailsDto> Items { get; init; } = new();
        public DeliveryDetailsDto? Delivery { get; init; }
        public PaymentDetailsDto? Payment { get; init; }
    }
}
