using OrderService.Models.enums;

namespace OrderService.Features.Commands.ReOrder
{
    public record ReOrderResultDto
    {
        public int NewOrderId { get; init; }
        public string NewOrderNumber { get; init; } = default!;
        public string OriginalOrderNumber { get; init; } = default!;
        public OrderStatus Status { get; init; }
        public decimal SubTotal { get; init; }
        public decimal DeliveryFee { get; init; }
        public decimal Tax { get; init; }
        public decimal Total { get; init; }
        public int ItemsCount { get; init; }
        public DateTime CreatedAt { get; init; }
        public List<ReOrderItemDto> Items { get; init; } = new();
    }
}
