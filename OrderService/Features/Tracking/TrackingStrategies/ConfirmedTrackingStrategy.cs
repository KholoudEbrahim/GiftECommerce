using OrderService.Models;
using OrderService.Models.enums;
using static OrderService.Features.Queries.TrackOrder.TrackOrderQueryHandler;
using OrderService.Features.Tracking.DTOs;
namespace OrderService.Features.Tracking.TrackingStrategies
{
    public class ConfirmedTrackingStrategy : ITrackingStrategy
    {
        public bool CanHandle(OrderStatus status) => status == OrderStatus.Confirmed;

        public OrderStatusTimelineDto CreateTimelineEntry(Order order, Delivery? delivery)
        {
            if (order.PaymentStatus == PaymentStatus.Completed && order.UpdatedAt.HasValue)
            {
                return new OrderStatusTimelineDto(
                    Status: OrderStatus.Confirmed,
                    Timestamp: order.UpdatedAt.Value
                );
            }
            return null;
        }
    }
}
