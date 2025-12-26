using OrderService.Models.enums;

namespace OrderService.Features.Tracking.DTOs
{
    public record OrderStatusTimelineDto(
        OrderStatus Status,
        DateTime Timestamp);

}
