using FluentValidation;
using FluentValidation.Validators;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OccasionService.Data;
using OccasionService.Features.CreateOccasion;
using OccasionService.Features.DeleteOccasion;
using OccasionService.Features.ToggleOccasionStatus;
using OccasionService.Features.UpdateOccasion;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OccasionDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

builder.Services.AddScoped<OccasionRepository>();


builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

builder.Services.AddScoped<IValidator<CreateOccasionCommand>, CreateOccasionValidator>();
builder.Services.AddScoped<IValidator<UpdateOccasionCommand>, UpdateOccasionValidator>();


builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
       
        cfg.ConfigureEndpoints(context);
    });
});
// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Occasion Service API",
        Version = "v1",
        Description = "Microservice for managing occasions (Wedding, Birthday, etc.)"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Occasion Service API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new
{
    service = "OccasionService",
    status = "Running",
    version = "1.0.0",
    timestamp = DateTime.UtcNow,
    endpoints = new[]
    {
        "GET /health - Health check",
        "POST /api/occasions - Create occasion",
        "PUT /api/occasions/{id} - Update occasion",
        "DELETE /api/occasions/{id} - Delete occasion",
        "PATCH /api/occasions/{id}/activate - Activate occasion",
        "PATCH /api/occasions/{id}/deactivate - Deactivate occasion"
    }
}))
.WithName("Root")
.WithTags("Info")
.ExcludeFromDescription();



app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "OccasionService",
    timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck")
.WithTags("Health");


app.MapCreateOccasion();
app.MapUpdateOccasion();
app.MapDeleteOccasion();
app.MapToggleOccasionStatus();


try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OccasionDbContext>();

        Console.WriteLine("⏳ Applying database migrations...");
        await dbContext.Database.EnsureCreatedAsync();
        dbContext.Database.Migrate();
        Console.WriteLine("✅ Database migrations applied successfully!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error applying migrations: {ex.Message}");
}


app.Run();
