using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderService.Data;
using OrderService.Features.Endpoints;
using OrderService.Middleware;
using OrderService.Services.Payment;
using Stripe;

namespace OrderService.Extensions
{
    public static class ApplicationBuilderExtensions
    {
     

        public static async Task<WebApplication> ApplyDatabaseMigrationsAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            return app;
        }

        public static WebApplication UseCustomMiddleware(this WebApplication app)
        {
            app.UseExceptionHandler();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            return app;
        }

        public static WebApplication UseStandardMiddleware(this WebApplication app)
        {
            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }

        public static WebApplication MapApplicationEndpoints(this WebApplication app)
        {
            app.MapOrderEndpoints();
            app.MapHealthChecks("/health");

            return app;
        }

        public static WebApplication MapStripeWebhook(this WebApplication app)
        {
            app.MapPost("/stripe-webhook", async (HttpContext context) =>
            {
                var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
                var stripeSignature = context.Request.Headers["Stripe-Signature"];

                var stripeSettings = context.RequestServices.GetRequiredService<IOptions<StripeSettings>>().Value;
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                try
                {
                    var stripeEvent = EventUtility.ConstructEvent(
                        json,
                        stripeSignature,
                        stripeSettings.WebhookSecret
                    );

                    // Handle different event types
                    switch (stripeEvent.Type)
                    {
                        case "payment_intent.succeeded":
                            var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
                            logger.LogInformation("Payment succeeded for payment intent: {PaymentIntentId}",
                                paymentIntent?.Id);
                            break;

                        case "payment_intent.payment_failed":
                            var failedPaymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
                            logger.LogWarning("Payment failed for payment intent: {PaymentIntentId}",
                                failedPaymentIntent?.Id);
                            break;

                        case "charge.refunded":
                            var charge = stripeEvent.Data.Object as Stripe.Charge;
                            logger.LogInformation("Charge refunded: {ChargeId}",
                                charge?.Id);
                            break;

                        default:
                            logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                            break;
                    }

                    return Results.Ok();
                }
                catch (StripeException ex)
                {
                    logger.LogError(ex, "Stripe webhook error");
                    return Results.BadRequest();
                }
            })
            .AllowAnonymous()
            .WithTags("Webhooks");

            return app;
        }
    }
}