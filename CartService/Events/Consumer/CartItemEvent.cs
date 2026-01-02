namespace CartService.Events.Consumer
{
    public record CartItemEvent
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = default!;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
