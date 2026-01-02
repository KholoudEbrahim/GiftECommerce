using OrderService.Data;
using OrderService.Events;
using OrderService.Events.Publisher;
using OrderService.Models.enums;
using Stripe;

namespace OrderService.Features.Endpoints
{
    public static class StripeWebhookEndpoint
    {
        public static RouteGroupBuilder MapStripeWebhook(this RouteGroupBuilder group)
        {
            group.MapPost("/stripe", HandleStripeWebhook)
                .WithName("HandleStripeWebhook")
                .WithOpenApi();

            return group;
        }

        private static async Task<IResult> HandleStripeWebhook(
             HttpContext context,
            IOrderRepository orderRepository,
             IEventPublisher eventPublisher,
             IConfiguration configuration,
             ILogger logger)
        {
            var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var webhookSecret = configuration["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    context.Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                logger.LogInformation("Received Stripe webhook: {EventType} {EventId}",
                    stripeEvent.Type, stripeEvent.Id);

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceeded(stripeEvent, orderRepository, eventPublisher, logger);
                        break;

                    case "payment_intent.payment_failed":
                        await HandlePaymentIntentFailed(stripeEvent, orderRepository, eventPublisher, logger);
                        break;

                    case "charge.refunded":
                        await HandleChargeRefunded(stripeEvent, logger);
                        break;

                    default:
                        logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Results.Ok();
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Stripe webhook validation failed");
                return Results.BadRequest();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Stripe webhook");
                return Results.StatusCode(500);
            }
        }

        private static async Task HandlePaymentIntentSucceeded(
            Stripe.Event stripeEvent,
            IOrderRepository orderRepository,
            IEventPublisher eventPublisher,
            ILogger logger)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

            if (paymentIntent == null) return;


            var orderNumber = paymentIntent.Metadata["order_number"];

            if (string.IsNullOrEmpty(orderNumber))
            {
                logger.LogWarning("Payment intent {PaymentIntentId} missing order number",
                    paymentIntent.Id);
                return;
            }

            var order = await orderRepository.GetOrderWithDetailsAsync(orderNumber);
            if (order == null)
            {
                logger.LogWarning("Order {OrderNumber} not found for payment intent {PaymentIntentId}",
                    orderNumber, paymentIntent.Id);
                return;
            }

  
            var payment = await orderRepository.GetPaymentByOrderIdAsync(order.Id);
            if (payment == null)
            {
                logger.LogWarning("Payment not found for order {OrderNumber}", orderNumber);
                return;
            }

            var oldStatus = order.PaymentStatus;


            string? cardLastFour = null;
        
            if (!string.IsNullOrEmpty(paymentIntent.LatestChargeId))
            {
                var chargeService = new ChargeService();
                var charge = await chargeService.GetAsync(paymentIntent.LatestChargeId);
                cardLastFour = charge?.PaymentMethodDetails?.Card?.Last4;
            }

            payment.MarkAsCompleted(paymentIntent.Id, cardLastFour);

            order.UpdatePaymentStatus(PaymentStatus.Completed);

            await orderRepository.UpdatePaymentAsync(payment);
            await orderRepository.UpdateAsync(order);
            await orderRepository.SaveChangesAsync();

            // Publish events
            await eventPublisher.PublishPaymentCompletedAsync(order, payment);

            if (oldStatus != PaymentStatus.Completed)
            {
                await eventPublisher.PublishOrderStatusUpdatedAsync(
                    order,
                    oldStatus.ToString()
                );
            }

            logger.LogInformation("Payment completed for order {OrderNumber}", orderNumber);
        }

        private static async Task HandlePaymentIntentFailed(
            Stripe.Event stripeEvent,
            IOrderRepository orderRepository,
            IEventPublisher eventPublisher,
            ILogger logger)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

            if (paymentIntent == null) return;

            var orderNumber = paymentIntent.Metadata["order_number"];
            if (string.IsNullOrEmpty(orderNumber)) return;

            var order = await orderRepository.GetOrderWithDetailsAsync(orderNumber);
            if (order == null) return;

            var payment = await orderRepository.GetPaymentByOrderIdAsync(order.Id);
            if (payment == null) return;

        
            payment.MarkAsFailed(paymentIntent.LastPaymentError?.Message ?? "Payment failed");
            order.UpdatePaymentStatus(PaymentStatus.Failed);

            await orderRepository.UpdatePaymentAsync(payment);
            await orderRepository.UpdateAsync(order);
            await orderRepository.SaveChangesAsync();

      
            var items = order.Items.Select(i => new InventoryItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList();

            await eventPublisher.PublishInventoryRollbackAsync(order.Id, order.OrderNumber, items);

            logger.LogWarning("Payment failed for order {OrderNumber}: {Reason}",
                orderNumber, payment.FailureReason);
        }

        private static Task HandleChargeRefunded(Stripe.Event stripeEvent, ILogger logger)
        {
            var charge = stripeEvent.Data.Object as Charge;
            logger.LogInformation("Refund processed for charge {ChargeId}", charge?.Id);
            return Task.CompletedTask;
        }
    }
}