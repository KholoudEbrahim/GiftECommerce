using OrderService.Features.Queries.TrackOrder;

namespace OrderService.Features.Tracking.DTOs
{
    public record DeliveryHeroDto
    {
        public string Name { get; init; } = default!;
        public string? Phone { get; init; }
        public LocationDto? CurrentLocation { get; init; }
    }
}
