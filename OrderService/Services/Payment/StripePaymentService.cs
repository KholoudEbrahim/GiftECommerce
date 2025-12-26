using Microsoft.Extensions.Options;
using Stripe;

namespace OrderService.Services.Payment
{
    public class StripePaymentService : IPaymentService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IOptions<StripeSettings> stripeSettings, ILogger<StripePaymentService> logger)
        {
            _stripeSettings = stripeSettings.Value;
            _logger = logger;

            // Configure Stripe
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(
            decimal amount,
            string currency,
            string customerEmail,
            string orderNumber,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = ConvertToCents(amount),
                    Currency = currency.ToLower(),
                    PaymentMethodTypes = new List<string> { "card" },
                    Description = $"Order: {orderNumber}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "order_number", orderNumber },
                        { "customer_email", customerEmail }
                    },
                    ReceiptEmail = customerEmail,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

                _logger.LogInformation("Created payment intent {PaymentIntentId} for order {OrderNumber}",
                    paymentIntent.Id, orderNumber);

                return paymentIntent;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create payment intent for order {OrderNumber}", orderNumber);
                throw new ApplicationException($"Payment intent creation failed: {ex.Message}", ex);
            }
        }

        public async Task<PaymentIntent> ConfirmPaymentIntentAsync(
            string paymentIntentId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.ConfirmAsync(
                    paymentIntentId,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Confirmed payment intent {PaymentIntentId} with status {Status}",
                    paymentIntent.Id, paymentIntent.Status);

                return paymentIntent;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to confirm payment intent {PaymentIntentId}", paymentIntentId);
                throw new ApplicationException($"Payment confirmation failed: {ex.Message}", ex);
            }
        }

        public async Task<PaymentIntent> GetPaymentIntentAsync(
            string paymentIntentId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var service = new PaymentIntentService();
                return await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to get payment intent {PaymentIntentId}", paymentIntentId);
                throw new ApplicationException($"Failed to retrieve payment intent: {ex.Message}", ex);
            }
        }

        public async Task RefundPaymentAsync(
            string paymentIntentId,
            decimal? amount = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Amount = amount.HasValue ? ConvertToCents(amount.Value) : null
                };

                var service = new RefundService();
                var refund = await service.CreateAsync(refundOptions, cancellationToken: cancellationToken);

                _logger.LogInformation("Created refund {RefundId} for payment intent {PaymentIntentId}",
                    refund.Id, paymentIntentId);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create refund for payment intent {PaymentIntentId}", paymentIntentId);
                throw new ApplicationException($"Refund creation failed: {ex.Message}", ex);
            }
        }

        private static long ConvertToCents(decimal amount)
        {
            return (long)(amount * 100);
        }
    }

    public class StripeSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }
}
