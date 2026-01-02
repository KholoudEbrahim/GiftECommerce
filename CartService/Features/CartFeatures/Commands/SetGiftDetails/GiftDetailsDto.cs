namespace CartService.Features.CartFeatures.Commands.SetGiftDetails
{
    public record GiftDetailsDto
    {
        public bool IsGift { get; init; }
        public string? RecipientName { get; init; }
        public string? RecipientPhone { get; init; }
        public string? GiftMessage { get; init; }
        public DateTime? DeliveryDate { get; init; }
        public bool GiftWrapRequested { get; init; }
        public decimal GiftWrapFee { get; init; }
    }
}
