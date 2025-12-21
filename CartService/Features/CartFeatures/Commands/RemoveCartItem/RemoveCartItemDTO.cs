namespace CartService.Features.CartFeatures.Commands.RemoveCartItem
{
    public record RemoveCartItemDTO
    {
        public Guid CartId { get; init; }
        public Guid ProductId { get; init; }
        public decimal SubTotal { get; init; }
        public decimal Total { get; init; }
    }
}
