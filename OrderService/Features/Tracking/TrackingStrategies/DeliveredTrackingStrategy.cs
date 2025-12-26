using OrderService.Features.Tracking.DTOs;
using OrderService.Models;
using OrderService.Models.enums;
using static OrderService.Features.Queries.TrackOrder.TrackOrderQueryHandler;

namespace OrderService.Features.Tracking.TrackingStrategies
{
    public class DeliveredTrackingStrategy : ITrackingStrategy
    {
        public bool CanHandle(OrderStatus status) => status == OrderStatus.Delivered;

        public OrderStatusTimelineDto CreateTimelineEntry(Order order, Delivery? delivery)
        {
            if (delivery?.Status == DeliveryStatus.Delivered && delivery.ActualDeliveryTime.HasValue)
            {
                return new OrderStatusTimelineDto(
                    Status: OrderStatus.Delivered,
                    Timestamp: delivery.ActualDeliveryTime.Value
                );
            }
            return null;
        }
    }

}
