namespace OrderService.Features.Queries.GetOrderById
{
    public record DeliveryDetailsDto
    {
        public Models.enums.DeliveryStatus Status { get; init; }
        public DateTime? EstimatedDeliveryTime { get; init; }
        public DateTime? ActualDeliveryTime { get; init; }
        public string? DeliveryHeroName { get; init; }
        public string? DeliveryHeroPhone { get; init; }
        public string? TrackingUrl { get; init; }
    }
}
