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
    }
}
