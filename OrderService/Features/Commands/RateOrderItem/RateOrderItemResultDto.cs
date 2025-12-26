namespace OrderService.Features.Commands.RateOrderItem
{
    public record RateOrderItemResultDto
    {
        public int RatingId { get; init; }
        public int OrderItemId { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = default!;
        public int Rating { get; init; }
        public string? Comment { get; init; }
        public DateTime RatedAt { get; init; }
    }

}
