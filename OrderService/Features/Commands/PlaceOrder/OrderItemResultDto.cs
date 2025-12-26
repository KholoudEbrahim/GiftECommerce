namespace OrderService.Features.Commands.PlaceOrder
{
    public record OrderItemResultDto
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = default!;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal TotalPrice { get; init; }
        public string? ImageUrl { get; init; }
    }
}
