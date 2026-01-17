using Microsoft.Extensions.Options;
using OrderService.Data;
using OrderService.Events;
using OrderService.Events.Publisher;
using OrderService.Models.enums;
using OrderService.Services.Payment;
using Stripe;

namespace OrderService.Features.Endpoints
{
    public static class StripeWebhookEndpoint
    {
        public static IEndpointRouteBuilder MapStripeWebhook(this IEndpointRouteBuilder app)
        {
            var webhookGroup = app.MapGroup("/api/webhooks")
                .WithTags("Webhooks")
                .AllowAnonymous();

            webhookGroup.MapPost("/stripe", HandleStripeWebhook)
                .WithName("HandleStripeWebhook")
                .WithSummary("Stripe webhook endpoint")
                .WithOpenApi()
                .ExcludeFromDescription();

            return app;
        }

        private static async Task<IResult> HandleStripeWebhook(
            HttpContext context,
            IOrderRepository orderRepository,
            IEventPublisher eventPublisher,
            IOptions<StripeSettings> stripeSettings,
            ILoggerFactory loggerFactory) 
        {

            var logger = loggerFactory.CreateLogger("StripeWebhook");

            var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var stripeSignature = context.Request.Headers["Stripe-Signature"].ToString();
            var webhookSecret = stripeSettings.Value.WebhookSecret;

            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                logger.LogError("Stripe webhook secret is not configured");
                return Results.Problem(
                    "Webhook configuration error",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            if (string.IsNullOrWhiteSpace(stripeSignature))
            {
                logger.LogWarning("Stripe webhook received without signature");
                return Results.BadRequest("Missing Stripe signature");
            }

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    webhookSecret
                );

                logger.LogInformation(
                    "Received Stripe webhook: {EventType} {EventId}",
                    stripeEvent.Type, stripeEvent.Id);

 
                var handled = stripeEvent.Type switch
                {
                    "payment_intent.succeeded" =>
                        await HandlePaymentIntentSucceeded(stripeEvent, orderRepository, eventPublisher, logger),

                    "payment_intent.payment_failed" =>
                        await HandlePaymentIntentFailed(stripeEvent, orderRepository, eventPublisher, logger),

                    "charge.refunded" =>
                        await HandleChargeRefunded(stripeEvent, orderRepository, eventPublisher, logger),

                    _ => HandleUnknownEvent(stripeEvent, logger)
                };

                return Results.Ok(new { received = true });
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Stripe webhook signature validation failed");
                return Results.BadRequest(new { error = "Invalid signature" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Stripe webhook");
                return Results.Problem(
                    "Webhook processing error",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

 
        private static async Task<bool> HandlePaymentIntentSucceeded(
            Stripe.Event stripeEvent,
            IOrderRepository orderRepository,
            IEventPublisher eventPublisher,
            ILogger logger)  
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                logger.LogWarning("PaymentIntent object is null in event {EventId}", stripeEvent.Id);
                return false;
            }

            if (!paymentIntent.Metadata.TryGetValue("order_number", out var orderNumber))
            {
                logger.LogWarning(
                    "PaymentIntent {PaymentIntentId} missing order_number metadata",
                    paymentIntent.Id);
                return false;
            }

            var order = await orderRepository.GetOrderWithDetailsAsync(orderNumber);
            if (order == null)
            {
                logger.LogWarning(
                    "Order {OrderNumber} not found for PaymentIntent {PaymentIntentId}",
                    orderNumber, paymentIntent.Id);
                return false;
            }

            var payment = await orderRepository.GetPaymentByOrderIdAsync(order.Id);
            if (payment == null)
            {
                logger.LogWarning(
                    "Payment not found for order {OrderNumber}",
                    orderNumber);
                return false;
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                logger.LogInformation(
                    "Payment already completed for order {OrderNumber}, webhook {EventId} ignored (idempotent)",
                    orderNumber, stripeEvent.Id);
                return true;
            }

            string? cardLastFour = null;
            if (!string.IsNullOrEmpty(paymentIntent.LatestChargeId))
            {
                try
                {
                    var chargeService = new ChargeService();
                    var charge = await chargeService.GetAsync(paymentIntent.LatestChargeId);
                    cardLastFour = charge?.PaymentMethodDetails?.Card?.Last4;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Failed to get card details for charge {ChargeId}",
                        paymentIntent.LatestChargeId);
                }
            }

            var oldOrderStatus = order.Status;

            payment.MarkAsCompleted(
                paymentIntent.LatestChargeId ?? paymentIntent.Id,
                cardLastFour);

            order.UpdatePaymentStatus(PaymentStatus.Completed);

            await orderRepository.UpdatePaymentAsync(payment);
            await orderRepository.UpdateAsync(order);
            await orderRepository.SaveChangesAsync();

            try
            {
                await eventPublisher.PublishPaymentCompletedAsync(order, payment);

                if (oldOrderStatus != order.Status)
                {
                    await eventPublisher.PublishOrderStatusUpdatedAsync(
                        order,
                        oldOrderStatus.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to publish events for order {OrderNumber} after payment success",
                    orderNumber);
            }

            logger.LogInformation(
                "Stripe payment completed successfully for order {OrderNumber}. " +
                "PaymentIntent: {PaymentIntentId}, Charge: {ChargeId}",
                orderNumber, paymentIntent.Id, paymentIntent.LatestChargeId);

            return true;
        }

        private static async Task<bool> HandlePaymentIntentFailed(
            Stripe.Event stripeEvent,
            IOrderRepository orderRepository,
            IEventPublisher eventPublisher,
            ILogger logger)  
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                logger.LogWarning("PaymentIntent object is null in event {EventId}", stripeEvent.Id);
                return false;
            }

            if (!paymentIntent.Metadata.TryGetValue("order_number", out var orderNumber))
            {
                logger.LogWarning(
                    "PaymentIntent {PaymentIntentId} missing order_number metadata",
                    paymentIntent.Id);
                return false;
            }

            var order = await orderRepository.GetOrderWithDetailsAsync(orderNumber);
            if (order == null)
            {
                logger.LogWarning("Order {OrderNumber} not found", orderNumber);
                return false;
            }

            var payment = await orderRepository.GetPaymentByOrderIdAsync(order.Id);
            if (payment == null)
            {
                logger.LogWarning("Payment not found for order {OrderNumber}", orderNumber);
                return false;
            }

            if (payment.Status == PaymentStatus.Failed)
            {
                logger.LogInformation(
                    "Payment already marked as failed for order {OrderNumber}, webhook ignored",
                    orderNumber);
                return true;
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                logger.LogWarning(
                    "Received payment_failed webhook for already completed order {OrderNumber}. Ignoring.",
                    orderNumber);
                return true;
            }

            var failureReason = paymentIntent.LastPaymentError?.Message
                ?? paymentIntent.CancellationReason
                ?? "Payment failed";

            payment.MarkAsFailed(failureReason);
            order.UpdatePaymentStatus(PaymentStatus.Failed);

            await orderRepository.UpdatePaymentAsync(payment);
            await orderRepository.UpdateAsync(order);
            await orderRepository.SaveChangesAsync();

            try
            {
                var items = order.Items.Select(i => new InventoryItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList();

                await eventPublisher.PublishInventoryRollbackAsync(
                    order.Id,
                    order.OrderNumber,
                    items);

                logger.LogInformation(
                    "Inventory rollback published for failed payment on order {OrderNumber}",
                    orderNumber);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to publish inventory rollback for order {OrderNumber}",
                    orderNumber);
            }

            logger.LogWarning(
                "Stripe payment failed for order {OrderNumber}. Reason: {FailureReason}",
                orderNumber, failureReason);

            return true;
        }

        private static async Task<bool> HandleChargeRefunded(
            Stripe.Event stripeEvent,
            IOrderRepository orderRepository,
            IEventPublisher eventPublisher,
            ILogger logger)  
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null)
            {
                logger.LogWarning("Charge object is null in event {EventId}", stripeEvent.Id);
                return false;
            }

            logger.LogInformation(
                "Processing refund for charge {ChargeId}. Refunded amount: {Amount}",
                charge.Id, charge.AmountRefunded);

            try
            {
                var order = await orderRepository.GetOrderByChargeIdAsync(charge.Id);

                if (order == null)
                {
                    logger.LogWarning(
                        "Order not found for charge {ChargeId}. This may be a refund for a non-order payment.",
                        charge.Id);
                    return false;
                }

                var payment = await orderRepository.GetPaymentByOrderIdAsync(order.Id);
                if (payment == null)
                {
                    logger.LogWarning(
                        "Payment not found for order {OrderNumber}",
                        order.OrderNumber);
                    return false;
                }

                if (payment.Status == PaymentStatus.Refunded &&
                    payment.RefundAmount >= (charge.AmountRefunded / 100m))
                {
                    logger.LogInformation(
                        "Refund already processed for order {OrderNumber}, webhook ignored",
                        order.OrderNumber);
                    return true;
                }

                var refundAmountInCurrency = charge.AmountRefunded / 100m;
                var isFullRefund = refundAmountInCurrency >= payment.Amount;

                string? refundId = null;
                string? refundReason = null;

                if (charge.Refunds?.Data?.Any() == true)
                {
                    var latestRefund = charge.Refunds.Data
                        .OrderByDescending(r => r.Created)
                        .FirstOrDefault();

                    refundId = latestRefund?.Id;
                    refundReason = latestRefund?.Reason ?? "Customer requested refund";
                }

                refundId ??= $"refund_{charge.Id}";

                var oldOrderStatus = order.Status;

                if (isFullRefund)
                {
                    payment.MarkAsRefunded(refundId, refundAmountInCurrency, refundReason);
                    order.UpdatePaymentStatus(PaymentStatus.Refunded);

                    if (order.Status != OrderStatus.Cancelled &&
                        order.Status != OrderStatus.Delivered)
                    {
                        order.UpdateStatus(OrderStatus.Cancelled);
                        order.MarkAsRefunded(refundReason ?? "Refund processed");  
                    }
                }
                else
                {
                    payment.MarkAsPartiallyRefunded(refundId, refundAmountInCurrency, refundReason);
                }

                await orderRepository.UpdatePaymentAsync(payment);
                await orderRepository.UpdateAsync(order);
                await orderRepository.SaveChangesAsync();

                try
                {
                    await eventPublisher.PublishRefundCompletedAsync(
                        order,
                        payment,
                        refundId,
                        refundAmountInCurrency,
                        refundReason ?? "Refund processed",
                        CancellationToken.None);

                    if (oldOrderStatus != order.Status)
                    {
                        await eventPublisher.PublishOrderStatusUpdatedAsync(
                            order,
                            oldOrderStatus.ToString(),
                            CancellationToken.None);
                    }

                    var items = order.Items.Select(i => new InventoryItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList();

                    await eventPublisher.PublishInventoryRollbackAsync(
                        order.Id,
                        order.OrderNumber,
                        items,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Failed to publish refund events for order {OrderNumber}",
                        order.OrderNumber);
                }

                logger.LogInformation(
                    "Refund processed successfully for order {OrderNumber}. " +
                    "Amount: {Amount}, Type: {RefundType}, Refund ID: {RefundId}",
                    order.OrderNumber,
                    refundAmountInCurrency,
                    isFullRefund ? "Full" : "Partial",
                    refundId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error processing refund for charge {ChargeId}",
                    charge.Id);
                return false;
            }
        }

        private static bool HandleUnknownEvent(
            Stripe.Event stripeEvent,
            ILogger logger) 
        {
            logger.LogInformation(
                "Received unhandled Stripe event type: {EventType} {EventId}",
                stripeEvent.Type, stripeEvent.Id);

            return true;
        }
    }
}
