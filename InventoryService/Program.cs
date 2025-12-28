using Carter;
using FluentValidation;
using InventoryService.Contracts;
using InventoryService.DataBase;
using InventoryService.Extensions;
using MediatR;
using InventoryService.Behaviour;
using Microsoft.EntityFrameworkCore;

namespace InventoryService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================================================
            // 1. BASIC SERVICES
            // =========================================================
            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();

            // =========================================================
            // 2. DATABASE
            // =========================================================
            builder.Services.AddDbContext<InventoryDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // =========================================================
            // 3. REPOSITORIES & HTTP CLIENTS
            // =========================================================
            builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();

            builder.Services.AddHttpClient("CartService", client =>
            {
                var baseUrl = builder.Configuration["ExternalServices:CartService:BaseUrl"]
                    ?? "http://localhost:5341";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            // =========================================================
            // 4. MASSTRANSIT + RABBITMQ 🐰
            // =========================================================
            builder.Services.AddMassTransitConfiguration(builder.Configuration);

            // =========================================================
            // 5. MEDIATR + VALIDATION + CARTER
            // =========================================================
            var assembly = typeof(Program).Assembly;

            builder.Services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(assembly);
                // Register Transactional Middleware
                cfg.AddOpenBehavior(typeof(TransactionalMiddleware.TransactionPipelineBehavior<,>));
            });

            builder.Services.AddValidatorsFromAssembly(assembly);
            builder.Services.AddCarter(configurator: config => config.WithValidatorLifetime(ServiceLifetime.Scoped));

            // =========================================================
            // BUILD APP
            // =========================================================
            var app = builder.Build();


            // =========================================================
            // MIDDLEWARE
            // =========================================================
          
                app.UseSwagger();
                app.UseSwaggerUI();
        

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // =========================================================
            // ENDPOINTS
            // =========================================================

            var summaries = new[]
       {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast")
            .WithOpenApi();

            app.MapCarter();

            app.MapGet("/health", () => Results.Ok(new
            {
                service = "InventoryService",
                status = "Healthy",
                timestamp = DateTime.UtcNow
            }))
            .WithName("HealthCheck")
            .WithTags("Health");

            app.Run();
        }
    }
}
