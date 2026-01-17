using OrderService.Models.enums;

namespace OrderService.Models
{
    public class Payment : BaseEntity
    {
        public int OrderId { get; private set; }
        public PaymentMethod Method { get; private set; }
        public PaymentStatus Status { get; private set; }
        public decimal Amount { get; private set; }
        public string? TransactionId { get; private set; }
        public string? PaymentGatewayResponse { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public string? FailureReason { get; private set; }

        // Stripe specific
        public string? StripePaymentIntentId { get; private set; }
        public string? StripeCustomerId { get; private set; }
        public string? CardLastFour { get; private set; }
        public string? RefundId { get; private set; }
        public decimal? RefundAmount { get; private set; }
        public DateTime? RefundedAt { get; private set; }
        public string? RefundReason { get; private set; }

        // Navigation
        public Order Order { get; private set; } = default!;

        private Payment() { }

        public static Payment Create(
            int orderId,
            PaymentMethod method,
            decimal amount,
            string? transactionId = null)
        {
            return new Payment
            {
                OrderId = orderId,
                Method = method,
                Status = PaymentStatus.Pending,
                Amount = amount,
                TransactionId = transactionId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsProcessing(string paymentIntentId, string? customerId = null)
        {
            if (Method != PaymentMethod.CreditCard)
                throw new InvalidOperationException("Only credit card payments can be marked as processing");

            StripePaymentIntentId = paymentIntentId;
            StripeCustomerId = customerId;
            Status = PaymentStatus.Processing;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsCompleted(string transactionId, string? cardLastFour = null)
        {
            Status = PaymentStatus.Completed;
            TransactionId = transactionId;
            CardLastFour = cardLastFour;
            PaidAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string reason)
        {
            Status = PaymentStatus.Failed;
            FailureReason = reason;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateGatewayResponse(string response)
        {
            PaymentGatewayResponse = response;
            UpdatedAt = DateTime.UtcNow;
        }
        public void MarkAsAwaitingCashPayment()
        {
            if (Method != PaymentMethod.CashOnDelivery)
                throw new InvalidOperationException("Only cash on delivery payments can be marked as awaiting");

            Status = PaymentStatus.AwaitingCashPayment;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsCashPaymentVerified(string transactionId = null)
        {
            if (Method != PaymentMethod.CashOnDelivery)
                throw new InvalidOperationException("Only cash on delivery payments can be verified");

            if (Status != PaymentStatus.AwaitingCashPayment)
                throw new InvalidOperationException("Payment must be in awaiting state to be verified");

            Status = PaymentStatus.CashPaymentVerified;
            TransactionId = transactionId ?? $"COD-VERIFIED-{Guid.NewGuid()}";
            PaidAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        public void MarkAsRefunded(string refundId, decimal refundAmount, string? reason = null)
        {
            if (Status != PaymentStatus.Completed)
                throw new InvalidOperationException("Only completed payments can be refunded");

            Status = PaymentStatus.Refunded;
            RefundId = refundId;
            RefundAmount = refundAmount;
            RefundReason = reason;
            RefundedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsPartiallyRefunded(string refundId, decimal refundAmount, string? reason = null)
        {
            if (Status != PaymentStatus.Completed && Status != PaymentStatus.Refunded)
                throw new InvalidOperationException("Invalid payment status for partial refund");

            RefundId = refundId;
            RefundAmount = (RefundAmount ?? 0) + refundAmount;
            RefundReason = reason;
            RefundedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

            if (RefundAmount >= Amount)
            {
                Status = PaymentStatus.Refunded;
            }
        }
    }
}
