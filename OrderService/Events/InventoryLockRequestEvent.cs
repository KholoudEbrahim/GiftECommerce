namespace OrderService.Events
{
    public record InventoryLockRequestEvent
    {
        public Guid CorrelationId { get; init; }
        public int OrderId { get; init; }
        public string OrderNumber { get; init; } = default!;
        public List<InventoryItem> Items { get; init; } = new();
        public DateTime RequestedAt { get; init; }
    }
    public record InventoryItem
    {
        public int ProductId { get; init; }
        public int Quantity { get; init; }
    }

}
