using Carter;
using CategoryService.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OfferService.Contracts;
using OfferService.Database;
using OfferService.DataBase;
using OfferService.Service.Behaviour;


namespace OfferService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ==========================================
        // 1. DATABASE CONFIGURATION
        // ==========================================
        builder.Services.AddDbContext<OfferDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // ==========================================
        // 2. VSA & ARCHITECTURE DEPENDENCIES
        // ==========================================
        var assembly = typeof(Program).Assembly;

        // MediatR (Handlers)
        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
            // Register Transactional Middleware
            cfg.AddOpenBehavior(typeof(TransactionalMiddleware.TransactionPipelineBehavior<,>));
        });

        // Carter (Endpoints)
        builder.Services.AddCarter();

        // FluentValidation (Validators)
        builder.Services.AddValidatorsFromAssembly(assembly);

        // ==========================================
        // 3. APPLICATION SERVICES & REPOSITORIES
        // ==========================================
        // Register the Generic Repository
        builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

        builder.Services.AddScoped<IDbIntializer, DbInitializer>();

        // ==========================================
        // 4. API DOCUMENTATION (SWAGGER)
        // ==========================================
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.ToString().Replace("+", "."));
        });

        // Add Authorization (Required if you use [Authorize] later)
        builder.Services.AddAuthorization();

        var app = builder.Build();


        await app.IntializeDataBase();

        // ==========================================
        // 5. HTTP PIPELINE
        // ==========================================
      
            app.UseSwagger();
            app.UseSwaggerUI();
        

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Map all Carter endpoints defined in Features
        app.MapCarter();

        app.Run();
    }
}