namespace OrderService.Events
{
    public record OrderStatusUpdatedEvent
    {
        public int OrderId { get; init; }
        public string OrderNumber { get; init; } = default!;
        public Guid UserId { get; init; }
        public string OldStatus { get; init; } = default!;
        public string NewStatus { get; init; } = default!;
        public DateTime UpdatedAt { get; init; }
    }
}
