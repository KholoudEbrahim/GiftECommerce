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
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var context = services.GetRequiredService<OrderDbContext>();

                logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database");
                throw;
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
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }

        public static WebApplication MapApplicationEndpoints(this WebApplication app)
        {
  
            app.MapOrderEndpoints();

            app.MapStripeWebhook();

            app.MapHealthChecks("/health");

            return app;
        }
    }
}
