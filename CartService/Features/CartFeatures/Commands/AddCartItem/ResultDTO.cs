namespace CartService.Features.CartFeatures.Commands.AddCartItem
{
    public record ResultDTO
    {
        public Guid CartId { get; init; }
        public Guid ItemId { get; init; }
        public int TotalItems { get; init; }
        public decimal SubTotal { get; init; }
        public decimal Total { get; init; }
    }
}
