namespace OrderService.Features.Endpoints.DTOs
{
    public record VerifyCashPaymentRequest
    {
        public string? TransactionId { get; init; }
    }
}
