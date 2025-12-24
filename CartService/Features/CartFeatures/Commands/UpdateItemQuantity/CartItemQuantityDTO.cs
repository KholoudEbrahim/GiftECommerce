namespace CartService.Features.CartFeatures.Commands.UpdateItemQuantity
{
    public record CartItemQuantityDTO
    {
        public int CartId { get; init; }
        public int ItemId { get; init; }
        public int Quantity { get; init; }
        public decimal SubTotal { get; init; }
        public decimal Total { get; init; }
        public bool ItemRemoved { get; init; }
    }

}
