using OrderService.Features.Queries.TrackOrder;
using OrderService.Features.Tracking.DTOs;
using OrderService.Features.Tracking.TrackingStrategies;
using OrderService.Models;
using OrderService.Models.enums;
using static OrderService.Features.Queries.TrackOrder.TrackOrderQueryHandler;

namespace OrderService.Features.Tracking
{
    public class TrackingService : ITrackingService
    {
        private readonly ITrackingStrategyFactory _strategyFactory;
        private readonly ILogger<TrackingService> _logger;

        public TrackingService(
            ITrackingStrategyFactory strategyFactory,
            ILogger<TrackingService> logger)
        {
            _strategyFactory = strategyFactory;
            _logger = logger;
        }

        public TrackingResultDto TrackOrder(Order order)
        {
            try
            {
                var timeline = BuildStatusTimeline(order);

                return new TrackingResultDto
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    CurrentStatus = order.Status,
                    StatusTimeline = timeline,
                    EstimatedDeliveryTime = order.Delivery?.EstimatedDeliveryTime,
                    DeliveryHero = MapDeliveryHero(order.Delivery),
                    TrackingUrl = order.Delivery?.TrackingUrl,
                    Items = MapOrderItems(order.Items)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track order {OrderNumber}", order.OrderNumber);
                throw;
            }
        }

        public List<OrderStatusTimelineDto> BuildStatusTimeline(Order order)
        {
            var timeline = new List<OrderStatusTimelineDto>();

            var statuses = Enum.GetValues<OrderStatus>()
                .Where(s => s != OrderStatus.Cancelled && s != OrderStatus.Failed)
                .OrderBy(s => (int)s)
                .ToList();

            foreach (var status in statuses)
            {
                try
                {
                    var strategy = _strategyFactory.GetStrategy(status);
                    var timelineEntry = strategy.CreateTimelineEntry(order, order.Delivery);

                    if (timelineEntry != null)
                    {
                        timeline.Add(timelineEntry);
                    }
                }
                catch (NotImplementedException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create timeline entry for status {Status}", status);
                }
            }

            return timeline;
        }

        private DTOs.DeliveryHeroDto? MapDeliveryHero(Delivery? delivery)
        {
            if (delivery == null) return null;

            return new DTOs.DeliveryHeroDto
            {
                Name = delivery.DeliveryHeroName ?? "Not assigned",
                Phone = delivery.DeliveryHeroPhone,
                CurrentLocation = delivery.CurrentLatitude.HasValue && delivery.CurrentLongitude.HasValue
                    ? new DTOs.LocationDto
                    {
                        Latitude = delivery.CurrentLatitude.Value,
                        Longitude = delivery.CurrentLongitude.Value
                    }
                    : null
            };
        }

        private List<DTOs.OrderItemTrackDto> MapOrderItems(IEnumerable<OrderItem> items)
        {
            return items.Select(i => new DTOs.OrderItemTrackDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList();
        }
    }
}