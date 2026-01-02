using OrderService.Models.enums;

namespace OrderService.Features.Commands.VerifyCashPayment
{
    public record VerifyCashPaymentResultDto
    {
        public string OrderNumber { get; init; } = default!;
        public PaymentStatus PaymentStatus { get; init; }
        public OrderStatus OrderStatus { get; init; }
        public DateTime VerifiedAt { get; init; }
        public Guid VerifiedBy { get; init; }
    }
}
