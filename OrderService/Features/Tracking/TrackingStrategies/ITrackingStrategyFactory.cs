using OrderService.Models.enums;

namespace OrderService.Features.Tracking.TrackingStrategies
{
    public interface ITrackingStrategyFactory
    {
        ITrackingStrategy GetStrategy(OrderStatus status);
    }

}
