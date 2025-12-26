namespace OrderService.Events
{
    public record OrderPlacedEvent
    {
        public int OrderId { get; init; }
        public string OrderNumber { get; init; } = default!;
        public Guid UserId { get; init; }
        public decimal TotalAmount { get; init; }
        public string PaymentMethod { get; init; } = default!;
        public DateTime PlacedAt { get; init; }
        public List<OrderItemEvent> Items { get; init; } = new();
    }

}
