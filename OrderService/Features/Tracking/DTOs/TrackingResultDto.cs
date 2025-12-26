using OrderService.Features.Queries.TrackOrder;
using OrderService.Models.enums;

namespace OrderService.Features.Tracking.DTOs
{
    public record TrackingResultDto
    {
        public int OrderId { get; init; }
        public string OrderNumber { get; init; } = default!;
        public OrderStatus CurrentStatus { get; init; }
        public List<OrderStatusTimelineDto> StatusTimeline { get; init; } = new();
        public DateTime? EstimatedDeliveryTime { get; init; }
        public DeliveryHeroDto? DeliveryHero { get; init; }
        public string? TrackingUrl { get; init; }
        public List<OrderItemTrackDto> Items { get; init; } = new();
    }

}
