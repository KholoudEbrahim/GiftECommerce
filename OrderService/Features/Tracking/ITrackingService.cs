using  OrderService.Features.Queries.TrackOrder;
using OrderService.Features.Tracking.DTOs;
using OrderService.Models;
using static OrderService.Features.Queries.TrackOrder.TrackOrderQueryHandler;
namespace OrderService.Features.Tracking
{
    public interface ITrackingService
    {
        TrackingResultDto TrackOrder(Order order);
        List<OrderStatusTimelineDto> BuildStatusTimeline(Order order);
    }
}
