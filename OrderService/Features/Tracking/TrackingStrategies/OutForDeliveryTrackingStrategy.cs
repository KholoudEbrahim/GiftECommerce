using OrderService.Models;
using OrderService.Models.enums;
using static OrderService.Features.Queries.TrackOrder.TrackOrderQueryHandler;
using OrderService.Features.Tracking.DTOs;
namespace OrderService.Features.Tracking.TrackingStrategies
{
    public class OutForDeliveryTrackingStrategy : ITrackingStrategy
    {
        public bool CanHandle(OrderStatus status) => status == OrderStatus.OutForDelivery;

        public OrderStatusTimelineDto CreateTimelineEntry(Order order, Delivery? delivery)
        {
            if (delivery?.Status >= DeliveryStatus.InTransit && delivery.UpdatedAt.HasValue)
            {
                return new OrderStatusTimelineDto(
                    Status: OrderStatus.OutForDelivery,
                    Timestamp: delivery.UpdatedAt.Value
                );
            }
            return null;
        }
    }
}
