
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Extensions;
using OrderService.Features.Endpoints;
using OrderService.Middleware;
using Serilog;
using System.Text.Json.Serialization;

namespace OrderService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, loggerConfig) =>
            {
                loggerConfig
                    .ReadFrom.Configuration(context.Configuration)
                    .WriteTo.Console();
            });

            builder.Services
                .AddApplicationServices(builder.Configuration)
                .AddSwaggerDocumentation()
                .AddDatabaseContext(builder.Configuration)
                .AddRedisCaching(builder.Configuration)
                .AddMassTransitWithRabbitMQ(builder.Configuration)
                .AddHttpClients(builder.Configuration)
                .AddAuthenticationServices(builder.Configuration)
                .AddMediatRServices()
                .AddFluentValidation()
                .AddCorsPolicy(builder.Configuration)
                .AddHealthChecksConfiguration(builder.Configuration)
                .AddScopedServices()
                .AddConfigurationSettings(builder.Configuration)
                .AddStripeServices(builder.Configuration)
                .AddProblemDetailsConfiguration();

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(
                    new JsonStringEnumConverter()
                );
            });
            var app = builder.Build();

            // Print something no matter what
            Console.WriteLine($"ENV = {app.Environment.EnvironmentName}");

            // Swagger in Development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API V1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                var urls = app.Urls.Any() ? string.Join(", ", app.Urls) : "(no urls in app.Urls)";
                Console.WriteLine($"OrderService started. Listening on: {urls}");
                Log.Information("OrderService started. Listening on: {Urls}", urls);
            });

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseExceptionHandler();

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<RequestLoggingMiddleware>();

            app.MapOrderEndpoints();
            app.MapStripeWebhook();
            app.MapHealthChecks("/health");

            // Migrations: do not kill app in Development
            if (app.Environment.IsDevelopment())
            {
                try
                {
                    await ApplyDatabaseMigrationsAsync(app);
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Database migration failed on startup (Development). App will continue running.");
                }
            }
            else
            {
                await ApplyDatabaseMigrationsAsync(app);
            }

            try
            {
                Log.Information("Starting Order Service...");
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                // This MUST show now because we forced Console sink + Console.WriteLine
                Console.Error.WriteLine(ex);
                Log.Fatal(ex, "Order Service failed to start");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            var context = services.GetRequiredService<OrderDbContext>();

            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
    }
}
