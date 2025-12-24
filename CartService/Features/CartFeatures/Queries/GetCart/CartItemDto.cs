namespace CartService.Features.CartFeatures.Queries.GetCart
{
    public record CartItemDto
    {
        public int Id { get; init; }
        public int ProductId { get; init; }
        public string Name { get; init; } = default!;
        public decimal UnitPrice { get; init; }
        public string ImageUrl { get; init; } = default!;
        public int Quantity { get; init; }
        public decimal TotalPrice { get; init; }
    }

}
