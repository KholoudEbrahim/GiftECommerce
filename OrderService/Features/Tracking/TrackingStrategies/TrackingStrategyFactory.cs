using OrderService.Models.enums;
using OrderService.Features.Tracking.DTOs;
namespace OrderService.Features.Tracking.TrackingStrategies
{
    public class TrackingStrategyFactory : ITrackingStrategyFactory
    {
        private readonly Dictionary<OrderStatus, ITrackingStrategy> _strategies;

        public TrackingStrategyFactory(IServiceProvider serviceProvider)
        {
            _strategies = new Dictionary<OrderStatus, ITrackingStrategy>
            {
                [OrderStatus.Pending] = serviceProvider.GetRequiredService<PendingTrackingStrategy>(),
                [OrderStatus.Confirmed] = serviceProvider.GetRequiredService<ConfirmedTrackingStrategy>(),
                [OrderStatus.Processing] = serviceProvider.GetRequiredService<ProcessingTrackingStrategy>(),
                [OrderStatus.OutForDelivery] = serviceProvider.GetRequiredService<OutForDeliveryTrackingStrategy>(),
                [OrderStatus.Delivered] = serviceProvider.GetRequiredService<DeliveredTrackingStrategy>()
            };
        }

        public ITrackingStrategy GetStrategy(OrderStatus status)
        {
            if (_strategies.TryGetValue(status, out var strategy))
            {
                return strategy;
            }

            throw new NotImplementedException($"No tracking strategy found for status: {status}");
        }
    }
}
