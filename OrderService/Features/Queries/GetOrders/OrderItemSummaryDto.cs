namespace OrderService.Features.Queries.GetOrders
{
    public record OrderItemSummaryDto
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = default!;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public string? ImageUrl { get; init; }
    }
}
