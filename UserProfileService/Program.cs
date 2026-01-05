
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "User Profile Service", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DbContext
builder.Services.AddDbContext<UserProfileDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Validators
builder.Services.AddScoped<IValidator<AddDeliveryAddressCommand>, AddDeliveryAddressValidator>();
builder.Services.AddScoped<IValidator<UpdateProfileCommand>, UpdateProfileValidator>();
builder.Services.AddScoped<IValidator<RemoveDeliveryAddressCommand>, RemoveDeliveryAddressCommandValidator>();
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),

            NameClaimType = JwtRegisteredClaimNames.Sub, 
            RoleClaimType = ClaimTypes.Role
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/debug/token", (HttpContext ctx) =>
{
    return Results.Ok(new
    {
        IsAuthenticated = ctx.User.Identity?.IsAuthenticated,
        Claims = ctx.User.Claims.Select(c => new { c.Type, c.Value })
    });
}).RequireAuthorization();


app.MapGetUserProfileEndpoint();
app.MapUpdateProfileEndpoint();
app.MapAddDeliveryAddressEndpoint();
app.MapGetUserAddressesEndpoint();
app.MapRemoveDeliveryAddressEndpoint();

// Migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserProfileDbContext>();
    db.Database.Migrate();
}

app.Run();