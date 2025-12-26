namespace OrderService.Features.Commands.ReOrder
{
    public record ReOrderItemDto
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = default!;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal TotalPrice { get; init; }
    }
}
