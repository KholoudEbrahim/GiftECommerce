namespace CartService.Features.CartFeatures.Commands.AddCartItem
{
    public record ResultDTO
    {
        public int CartId { get; init; }     
        public int ItemId { get; init; }     
        public int TotalItems { get; init; }
        public decimal SubTotal { get; init; }
        public decimal Total { get; init; }
    }
}
