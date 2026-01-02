using CartService.Models.enums;

namespace CartService.Features.CartFeatures.Commands.SetGiftDetails
{
    public record AddressDetailsDto
    {
        public Guid AddressId { get; init; }
        public AddressType AddressType { get; init; }
        public string FullAddress { get; init; } = default!;
        public string? Landmark { get; init; }
        public bool IsDefault { get; init; }
    }
}
