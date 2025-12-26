using MediatR;
using OrderService.Data;
using OrderService.Models.enums;

namespace OrderService.Features.Queries.GetOrders
{
    public record GetOrdersQuery(
      Guid UserId,
      bool? ActiveOnly = null,
      int Page = 1,
      int PageSize = 20) : IRequest<GetOrdersResultDto>;

    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, GetOrdersResultDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<GetOrdersQueryHandler> _logger;

        public GetOrdersQueryHandler(
            IOrderRepository orderRepository,
            ILogger<GetOrdersQueryHandler> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<GetOrdersResultDto> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var orders = await _orderRepository.GetUserOrdersAsync(
                    request.UserId,
                    request.ActiveOnly,
                    cancellationToken
                );

                var totalCount = orders.Count();
                var pagedOrders = orders
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var orderDtos = pagedOrders.Select(MapToDto).ToList();

                var activeOrders = orderDtos
                    .Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
                    .ToList();

                var completedOrders = orderDtos
                    .Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Cancelled)
                    .ToList();

                _logger.LogInformation("Retrieved {Count} orders for user {UserId}",
                    orderDtos.Count, request.UserId);

                return new GetOrdersResultDto
                {
                    ActiveOrders = activeOrders,
                    CompletedOrders = completedOrders,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get orders for user {UserId}", request.UserId);
                throw;
            }
        }

        private OrderSummaryDto MapToDto(Models.Order order)
        {
            return new OrderSummaryDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                Total = order.Total,
                ItemCount = order.Items.Count,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.Items.Take(3).Select(i => new OrderItemSummaryDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    ImageUrl = i.ImageUrl
                }).ToList(),
                DeliveryDate = order.Delivery?.ActualDeliveryTime
            };
        }
    }

}
