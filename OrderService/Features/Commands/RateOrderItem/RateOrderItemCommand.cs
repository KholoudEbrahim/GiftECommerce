using MediatR;
using OrderService.Data;
using OrderService.Models;
using OrderService.Models.enums;

namespace OrderService.Features.Commands.RateOrderItem
{
    public record RateOrderItemCommand(
          Guid UserId,
          int OrderItemId,
          int Rating,
          string? Comment = null) : IRequest<RateOrderItemResultDto>;

    public class RateOrderItemCommandHandler : IRequestHandler<RateOrderItemCommand, RateOrderItemResultDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<RateOrderItemCommandHandler> _logger;

        public RateOrderItemCommandHandler(
            IOrderRepository orderRepository,
            ILogger<RateOrderItemCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<RateOrderItemResultDto> Handle(RateOrderItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
             
                var hasRated = await _orderRepository.HasUserRatedOrderItemAsync(
                    request.UserId,
                    request.OrderItemId,
                    cancellationToken);

                if (hasRated)
                    throw new InvalidOperationException("You have already rated this item");

    
                var orderItem = await _orderRepository.GetOrderItemWithOrderAsync(request.OrderItemId, cancellationToken);

                if (orderItem == null)
                    throw new KeyNotFoundException($"Order item {request.OrderItemId} not found");

                if (orderItem.Order.Status != OrderStatus.Delivered)
                    throw new InvalidOperationException("Only items from delivered orders can be rated");

                if (orderItem.Order.UserId != request.UserId)
                    throw new UnauthorizedAccessException("You can only rate items from your own orders");

                var rating = Rating.Create(
                    userId: request.UserId,
                    productId: orderItem.ProductId,
                    orderItemId: orderItem.Id,
                    score: request.Rating,
                    comment: request.Comment
                );

                await _orderRepository.AddRatingAsync(rating, cancellationToken);
                await _orderRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {UserId} rated item {OrderItemId} with score {Rating}",
                    request.UserId, request.OrderItemId, request.Rating);

                return new RateOrderItemResultDto
                {
                    RatingId = rating.Id,
                    OrderItemId = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    ProductName = orderItem.ProductName,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    RatedAt = rating.RatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rate order item {OrderItemId}", request.OrderItemId);
                throw;
            }
        }
    }
}