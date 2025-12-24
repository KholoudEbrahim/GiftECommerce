
using CartService.Features.Shared;
using CartService.Middleware;
using CartService.Data;
using CartService.Models;
using CartService.Extensions;
using CartService.Services;
using CartService.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cart Service API",
        Version = "v1",
        Description = "Microservice for managing shopping carts"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});



// Add database context
builder.Services.AddDbContext<CartDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CartDatabase"),
        sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(CartDbContext).Assembly.FullName);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

// Add Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});


// Add HTTP clients for external services
builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ExternalServicesSettings>>().Value;
    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutInSeconds);
    client.DefaultRequestHeaders.Add("User-Agent", "CartService");
})
    .AddPolicyHandler(HttpClientExtensions.GetRetryPolicy())
    .AddPolicyHandler(HttpClientExtensions.GetCircuitBreakerPolicy());

builder.Services.AddHttpClient<IProfileServiceClient, ProfileServiceClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ExternalServicesSettings>>().Value;
    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutInSeconds);
    client.DefaultRequestHeaders.Add("User-Agent", "CartService");
})
    .AddPolicyHandler(HttpClientExtensions.GetRetryPolicy())
    .AddPolicyHandler(HttpClientExtensions.GetCircuitBreakerPolicy());

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("CartDatabase")!,
        name: "cart-db-check",
        tags: new[] { "database", "sqlserver" })
    .AddRedis(
        builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379",
        name: "redis-check",
        tags: new[] { "cache", "redis" });

// Add scoped services
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddHttpContextAccessor();

// Add configuration
builder.Services.Configure<ExternalServicesSettings>(
    builder.Configuration.GetSection("ExternalServices"));

// Add problem details
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CartDbContext>();
    dbContext.Database.Migrate();
} 

await ApplyMigrationsAsync(app.Services);

app.UseExceptionHandler();

// Custom middleware
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Standard middleware
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapCartEndpoints();
app.MapHealthChecks("/health");

// Run the app
await app.RunAsync();

static async Task ApplyMigrationsAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CartDbContext>();
    await dbContext.Database.MigrateAsync();
}