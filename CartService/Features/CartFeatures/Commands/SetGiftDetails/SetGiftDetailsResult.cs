namespace CartService.Features.CartFeatures.Commands.SetGiftDetails
{
    public record SetGiftDetailsResult
    {
        public int CartId { get; init; }
        public bool IsGift { get; init; }
        public decimal GiftWrapFee { get; init; }
        public decimal Total { get; init; }
    }
}
