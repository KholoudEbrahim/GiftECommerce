using OrderService.Models.enums;

namespace OrderService.Features.Queries.GetOrders
{
    public record OrderSummaryDto
    {
        public int Id { get; init; }
        public string OrderNumber { get; init; } = default!;
        public OrderStatus Status { get; init; }
        public PaymentMethod PaymentMethod { get; init; }
        public decimal Total { get; init; }
        public int ItemCount { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public DateTime? DeliveryDate { get; init; }
        public List<OrderItemSummaryDto> Items { get; init; } = new();
    }
}
