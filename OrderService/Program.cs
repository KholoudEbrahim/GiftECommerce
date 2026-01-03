
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Extensions;
using OrderService.Features.Endpoints;
using OrderService.Middleware;
using Serilog;

namespace OrderService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Serilog
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));

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



            var app = builder.Build();



            app.UseSwagger();
                app.UseSwaggerUI();
         
        
             app.ApplyDatabaseMigrationsAsync().GetAwaiter().GetResult()
             .UseCustomMiddleware()
             .UseStandardMiddleware()
             .MapApplicationEndpoints()
             .MapStripeWebhook();

  
     

            app.UseExceptionHandler();

        
            app.UseCors("AllowFrontend");
        
            


            app.UseHttpsRedirection();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseExceptionHandler(); 
            app.UseAuthentication();
            app.UseAuthorization();

            // Map endpoints
 
            app.MapHealthChecks("/health");
            app.MapGroup("/api/webhooks")
           ;



            await app.RunAsync();
        }
    }
}
