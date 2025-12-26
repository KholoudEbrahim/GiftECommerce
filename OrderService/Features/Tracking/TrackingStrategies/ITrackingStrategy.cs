using OrderService.Features.Tracking.DTOs;
using OrderService.Models;
using OrderService.Models.enums;
using static OrderService.Features.Queries.TrackOrder.TrackOrderQueryHandler;

namespace OrderService.Features.Tracking.TrackingStrategies
{
    public interface ITrackingStrategy
    {
        bool CanHandle(OrderStatus status);
        OrderStatusTimelineDto CreateTimelineEntry(Order order, Delivery? delivery);
    }
}
