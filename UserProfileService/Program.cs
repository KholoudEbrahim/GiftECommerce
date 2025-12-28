
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserProfileService.Application;
using UserProfileService.Data;
using UserProfileService.Events;
using UserProfileService.Features.Commands.DeliveryAddress;
using UserProfileService.Features.Commands.RemoveDeliveryAddress;
using UserProfileService.Features.Commands.UpdateProfile;
using UserProfileService.Features.Queries.GetProfile;
using UserProfileService.Features.Queries.ListDeliveryAddresses;
using UserProfileService.Features.Shared;
using UserProfileService.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.AddApiResponseConfiguration();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "User Profile Service", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });
});

// Database Context
builder.Services.AddDbContext<UserProfileDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions =>
        {
            sqlServerOptions.MigrationsAssembly(typeof(UserProfileDbContext).Assembly.FullName);
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

builder.Services.AddScoped<IValidator<AddDeliveryAddressCommand>, AddDeliveryAddressValidator>();
builder.Services.AddScoped<IValidator<UpdateProfileCommand>, UpdateProfileValidator>();
builder.Services.AddScoped<IValidator<RemoveDeliveryAddressCommand>, RemoveDeliveryAddressCommandValidator> ();
builder.Services.AddScoped<IValidator<GetUserAddressesQuery>, GetUserAddressesQueryValidator>();
builder.Services.AddScoped<IValidator<GetUserProfileQuery>, GetUserProfileQueryValidator>();


builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumer<UserCreatedConsumer>()
        .Endpoint(e => e.ConcurrentMessageLimit = 3);

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

       
        cfg.UseMessageRetry(r =>
        {
            r.Interval(5, TimeSpan.FromSeconds(10));
            r.Handle<DbUpdateException>();
            r.Handle<TimeoutException>();
        });


        cfg.ReceiveEndpoint("user-created", e =>
        {
            e.PrefetchCount = 10;
            e.UseMessageRetry(r => r.Interval(3, 1000));
            e.ConfigureConsumer<UserCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);





var app = builder.Build();

// Configure the HTTP request pipeline

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");

// Map endpoints
app.MapGetUserProfileEndpoint();
app.MapUpdateProfileEndpoint();
app.MapAddDeliveryAddressEndpoint();
app.MapGetUserAddressesEndpoint();
app.MapRemoveDeliveryAddressEndpoint();


// Database migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserProfileDbContext>();
    dbContext.Database.Migrate();
}

app.Run();