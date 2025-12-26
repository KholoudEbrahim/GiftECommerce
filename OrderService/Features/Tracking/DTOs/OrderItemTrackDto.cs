namespace OrderService.Features.Tracking.DTOs
{
    public record OrderItemTrackDto
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = default!;
        public int Quantity { get; init; }
        public string? ImageUrl { get; init; }
    }
}
