using MediatR;
using OrderService.Data;

namespace OrderService.Features.Queries.GetOrderById
{
    public record GetOrderByIdQuery(
          Guid UserId,
          string OrderNumber) : IRequest<OrderDetailsDto>;

    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailsDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<GetOrderByIdQueryHandler> _logger;

        public GetOrderByIdQueryHandler(
            IOrderRepository orderRepository,
            ILogger<GetOrderByIdQueryHandler> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<OrderDetailsDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderNumber, cancellationToken);
                if (order == null || order.UserId != request.UserId)
                    throw new KeyNotFoundException($"Order {request.OrderNumber} not found");

                return MapToOrderDetailsDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get order {OrderNumber} for user {UserId}",
                    request.OrderNumber, request.UserId);
                throw;
            }
        }

        private OrderDetailsDto MapToOrderDetailsDto(Models.Order order)
        {
            return new OrderDetailsDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                SubTotal = order.SubTotal,
                DeliveryFee = order.DeliveryFee,
                Discount = order.Discount,
                Tax = order.Tax,
                Total = order.Total,
                DeliveryAddressId = order.DeliveryAddressId,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.Items.Select(i => new OrderItemDetailsDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    ImageUrl = i.ImageUrl,
                    Discount = i.Discount,
                    TotalPrice = i.TotalPrice,
                   
                    Rating = i.GetLatestRating(),     
                    RatingComment = i.GetLatestRatingComment(), 
                    RatedAt = i.GetLatestRatedAt()      
                }).ToList(),
                Delivery = order.Delivery != null ? new DeliveryDetailsDto
                {
                    Status = order.Delivery.Status,
                    EstimatedDeliveryTime = order.Delivery.EstimatedDeliveryTime,
                    ActualDeliveryTime = order.Delivery.ActualDeliveryTime,
                    DeliveryHeroName = order.Delivery.DeliveryHeroName,
                    DeliveryHeroPhone = order.Delivery.DeliveryHeroPhone,
                    TrackingUrl = order.Delivery.TrackingUrl
                } : null,
                Payment = order.Payment != null ? new PaymentDetailsDto
                {
                    Status = order.Payment.Status,
                    Amount = order.Payment.Amount,
                    TransactionId = order.Payment.TransactionId,
                    PaidAt = order.Payment.PaidAt,
                    CardLastFour = order.Payment.CardLastFour
                } : null
            };
        }
    }
}