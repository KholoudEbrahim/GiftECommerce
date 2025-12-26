namespace OrderService.Features.Queries.GetOrders
{
    public record GetOrdersResultDto
    {
        public List<OrderSummaryDto> ActiveOrders { get; init; } = new();
        public List<OrderSummaryDto> CompletedOrders { get; init; } = new();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
    }
  
}
