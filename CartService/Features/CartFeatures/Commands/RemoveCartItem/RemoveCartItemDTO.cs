namespace CartService.Features.CartFeatures.Commands.RemoveCartItem
{
    public record RemoveCartItemDTO
    {
        public int CartId { get; init; }
        public int ProductId { get; init; }
        public decimal SubTotal { get; init; }
        public decimal Total { get; init; }
    }
}
