namespace OrderService.Events
{
    public record InventoryLockResponseEvent
    {
        public Guid CorrelationId { get; init; }
        public int OrderId { get; init; }
        public bool Success { get; init; }
        public string? FailureReason { get; init; }
        public List<UnavailableItem>? UnavailableItems { get; init; }
        public DateTime RespondedAt { get; init; }
    }
    public record UnavailableItem
    {
        public int ProductId { get; init; }
        public int RequestedQuantity { get; init; }
        public int AvailableQuantity { get; init; }
    }
}
