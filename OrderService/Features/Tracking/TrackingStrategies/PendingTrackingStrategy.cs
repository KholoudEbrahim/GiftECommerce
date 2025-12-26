using OrderService.Models;
using OrderService.Models.enums;
using static OrderService.Features.Queries.TrackOrder.TrackOrderQueryHandler;
using OrderService.Features.Tracking.DTOs;
namespace OrderService.Features.Tracking.TrackingStrategies
{
    public class PendingTrackingStrategy : ITrackingStrategy
    {
        public bool CanHandle(OrderStatus status) => status == OrderStatus.Pending;

        public OrderStatusTimelineDto CreateTimelineEntry(Order order, Delivery? delivery)
        {
            return new OrderStatusTimelineDto(
                Status: OrderStatus.Pending,
                Timestamp: order.CreatedAt
            );
        }
    }
}
