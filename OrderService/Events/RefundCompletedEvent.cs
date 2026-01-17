namespace OrderService.Events
{

    public record RefundCompletedEvent
    {
        public int OrderId { get; init; }
        public string OrderNumber { get; init; } = default!;
        public Guid UserId { get; init; }
        public decimal RefundAmount { get; init; }
        public string RefundId { get; init; } = default!;
        public string Reason { get; init; } = default!;
        public DateTime RefundedAt { get; init; }
        public List<InventoryItem> ItemsToRestock { get; init; } = new();
    }
}
