namespace CartService.Features.CartFeatures.Commands.SetDeliveryAddress
{
    public record SelectDeliveryAddressDTO
    {
        public int CartId { get; init; }
        public Guid AddressId { get; init; }
        public decimal DeliveryFee { get; init; }
        public decimal Total { get; init; }
    }

}
