using OrderService.Models.enums;

namespace OrderService.Features.Endpoints.DTOs
{
    public record PlaceOrderRequest
    {
        public required Guid DeliveryAddressId { get; init; }
        public required PaymentMethod PaymentMethod { get; init; }
        public string? Notes { get; init; }
    }
}
