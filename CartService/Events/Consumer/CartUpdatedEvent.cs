namespace CartService.Events.Consumer
{
    public record CartUpdatedEvent
    {
        public int CartId { get; init; }
        public Guid? UserId { get; init; }
        public string? AnonymousId { get; init; }
        public decimal TotalAmount { get; init; }
        public int ItemCount { get; init; }
        public DateTime UpdatedAt { get; init; }
        public List<CartItemEvent> Items { get; init; } = new();
    }
}
