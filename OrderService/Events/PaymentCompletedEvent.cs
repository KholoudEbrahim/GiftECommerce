namespace OrderService.Events
{
    public record PaymentCompletedEvent
    {
        public int OrderId { get; init; }
        public string OrderNumber { get; init; } = default!;
        public Guid UserId { get; init; }
        public decimal Amount { get; init; }
        public string PaymentMethod { get; init; } = default!;
        public string TransactionId { get; init; } = default!;
        public DateTime CompletedAt { get; init; }
    }
}
