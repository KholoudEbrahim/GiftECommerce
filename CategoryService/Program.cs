using Carter;
using CategoryService.Behaviour;
using CategoryService.Contracts;
using CategoryService.Contracts.ExternalServices;
using CategoryService.DataBase;
using CategoryService.Extensions;
using CategoryService.shared.Services;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using System.Reflection;

namespace CategoryService;





public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        builder.Services.AddHttpContextAccessor();


        // =================================================================
        // 2. INFRASTRUCTURE (Database, Redis, AWS)
        // =================================================================
        // Database
        builder.Services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // =================================================================
        // 3. APPLICATION SERVICES (DI)
        // =================================================================

        // Generic Repositories & Helpers
        builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
        builder.Services.AddScoped<IDbIntializer, DbInitializer>();
        builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();



        builder.Services.AddMassTransitConfiguration(builder.Configuration);


        // =================================================================
        // 4. LIBRARIES (MediatR, Carter, Validation, Polly)
        // =================================================================
        var assembly = typeof(Program).Assembly;

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
            // Register Transactional Middleware
            cfg.AddOpenBehavior(typeof(TransactionalMiddleware.TransactionPipelineBehavior<,>));
        });

        builder.Services.AddValidatorsFromAssembly(assembly);
        builder.Services.AddCarter(configurator: config => config.WithValidatorLifetime(ServiceLifetime.Scoped));

        // Cart Service Client
        builder.Services.AddHttpClient<ICartServiceClient, CartServiceClient>(client =>
        {
            var baseUrl = builder.Configuration["ExternalServices:CartService:BaseUrl"]
                ?? "http://localhost:5004";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Order Service Client
        builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
        {
            var baseUrl = builder.Configuration["ExternalServices:OrderService:BaseUrl"]
                ?? "http://localhost:5005";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Inventory Service Client
        builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
        {
            var baseUrl = builder.Configuration["ExternalServices:InventoryService:BaseUrl"]
                ?? "http://localhost:5003";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());



        var app = builder.Build();
        
        // does the Migration(If Any) and Seeds The Db if it is Empty
        await app.IntializeDataBase();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

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
        app.Run();
    }

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s");
                });
    }

    static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    Console.WriteLine($"Circuit breaker opened for {timespan.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker reset");
                });
    }
}
