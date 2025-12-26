using MediatR;
using OrderService.Data;
using OrderService.Features.Tracking;
using OrderService.Features.Tracking.DTOs;
using OrderService.Models.enums;
using OrderService.Services.DTOs;

namespace OrderService.Features.Queries.TrackOrder
{
    public record TrackOrderQuery(
       Guid UserId,
       string OrderNumber) : IRequest<TrackingResultDto>;

    public class TrackOrderQueryHandler : IRequestHandler<TrackOrderQuery, TrackingResultDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITrackingService _trackingService;
        private readonly ILogger<TrackOrderQueryHandler> _logger;

        public TrackOrderQueryHandler(
            IOrderRepository orderRepository,
            ITrackingService trackingService,
            ILogger<TrackOrderQueryHandler> logger)
        {
            _orderRepository = orderRepository;
            _trackingService = trackingService;
            _logger = logger;
        }

        public async Task<TrackingResultDto> Handle(TrackOrderQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderNumber, cancellationToken);
                if (order == null || order.UserId != request.UserId)
                    throw new KeyNotFoundException($"Order {request.OrderNumber} not found");

            
                var trackingResult = _trackingService.TrackOrder(order);

                _logger.LogInformation("Successfully tracked order {OrderNumber}", order.OrderNumber);

                return trackingResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track order {OrderNumber}", request.OrderNumber);
                throw;
            }
        }
    }
}