using Carter;
using CategoryService.Behaviour;
using CategoryService.Contracts;
using CategoryService.DataBase;
using CategoryService.Extensions;
using CategoryService.shared.Services;
using FluentValidation;
using MediatR;
using RabbitMQ;
using MassTransit;
using Microsoft.EntityFrameworkCore;

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






        // =================================================================
        // 4. LIBRARIES (MediatR, Carter, Validation)
        // =================================================================
        var assembly = typeof(Program).Assembly;

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
            // Register Transactional Middleware
            cfg.AddOpenBehavior(typeof(TransactionalMiddleware.TransactionPipelineBehavior<,>));
        });

        builder.Services.AddValidatorsFromAssembly(assembly);
        builder.Services.AddCarter(configurator: config => config.WithValidatorLifetime(ServiceLifetime.Scoped));



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
}
