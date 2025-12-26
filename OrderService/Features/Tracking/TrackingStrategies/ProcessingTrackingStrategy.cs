using OrderService.Models;
using OrderService.Models.enums;
using static OrderService.Features.Queries.TrackOrder.TrackOrderQueryHandler;
using OrderService.Features.Tracking.DTOs;
namespace OrderService.Features.Tracking.TrackingStrategies
{
    public class ProcessingTrackingStrategy : ITrackingStrategy
    {
        public bool CanHandle(OrderStatus status) => status == OrderStatus.Processing;

        public OrderStatusTimelineDto CreateTimelineEntry(Order order, Delivery? delivery)
        {
            if (delivery?.Status >= DeliveryStatus.Assigned && delivery.UpdatedAt.HasValue)
            {
                return new OrderStatusTimelineDto(
                    Status: OrderStatus.Processing,
                    Timestamp: delivery.UpdatedAt.Value
                );
            }
            return null;
        }
    }
}
