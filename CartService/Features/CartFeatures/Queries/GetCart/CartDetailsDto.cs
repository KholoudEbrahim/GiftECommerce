using CartService.Models.enums;
using CartService.Services.DTOs;


namespace CartService.Features.CartFeatures.Queries.GetCart
{
    public record CartDetailsDto
    {
        public int CartId { get; init; }
        public Guid? UserId { get; init; }
        public string? AnonymousId { get; init; }
        public CartStatus Status { get; init; }
        public List<CartItemDto> Items { get; init; } = new();
        public decimal SubTotal { get; init; }
        public decimal DeliveryFee { get; init; }
        public decimal Total { get; init; }
        public Guid? DeliveryAddressId { get; init; }
        public AddressDto? DeliveryAddress { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
