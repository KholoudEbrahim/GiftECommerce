namespace OrderService.Features.Queries.GetOrderById
{
    public record OrderItemDetailsDto
    {
        public int Id { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = default!;
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
        public string? ImageUrl { get; init; }
        public decimal? Discount { get; init; }
        public decimal TotalPrice { get; init; }
        public int? Rating { get; init; }
        public string? RatingComment { get; init; }
        public DateTime? RatedAt { get; init; }
    }
}
