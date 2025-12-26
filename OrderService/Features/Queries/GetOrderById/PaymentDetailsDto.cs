namespace OrderService.Features.Queries.GetOrderById
{
    public record PaymentDetailsDto
    {
        public Models.enums.PaymentStatus Status { get; init; }
        public decimal Amount { get; init; }
        public string? TransactionId { get; init; }
        public DateTime? PaidAt { get; init; }
        public string? CardLastFour { get; init; }
    }
}
