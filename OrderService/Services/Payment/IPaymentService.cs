using Microsoft.Extensions.Options;
using Stripe;

namespace OrderService.Services.Payment
{
    public interface IPaymentService
    {
        Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string customerEmail,
            string orderNumber, CancellationToken cancellationToken = default);
        Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
        Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
        Task RefundPaymentAsync(string paymentIntentId, decimal? amount = null, CancellationToken cancellationToken = default);
    }


}
