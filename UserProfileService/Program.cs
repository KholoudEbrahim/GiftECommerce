
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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

// Configure logging for debugging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

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

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($" AUTH FAILED: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($" TOKEN VALID for: {context.Principal?.FindFirst("sub")?.Value}");
                return Task.CompletedTask;
            }
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


// Configure Kestrel to listen on all interfaces on port 8080
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ListenAnyIP(8080);
//});

//// Or use URLs configuration
//builder.WebHost.UseUrls("http://0.0.0.0:8080");


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");

// Debug endpoints
app.MapGet("/api/debug/token", (HttpContext httpContext) =>
{
    var token = httpContext.Request.Headers["Authorization"].ToString();
    var user = httpContext.User;

    return Results.Ok(new
    {
        HasToken = !string.IsNullOrEmpty(token),
        Token = token.Length > 50 ? token[..50] + "..." : token,
        UserId = user.FindFirst("sub")?.Value ?? user.FindFirst("userId")?.Value,
        UserName = user.Identity?.Name,
        IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
        Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList()
    });
}).RequireAuthorization(); 

app.MapGet("/api/health/public", () => Results.Ok(new
{
    Service = "UserProfileService",
    Status = "Running",
    Time = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName
}));

app.MapPost("/api/profile/init", async (HttpContext httpContext, IMediator mediator) =>
{
    try
    {

        var userIdClaim = httpContext.User.FindFirst("sub")?.Value
            ?? httpContext.User.FindFirst("userId")?.Value;

        Console.WriteLine($" Init endpoint called, userIdClaim: {userIdClaim}");
        Console.WriteLine($" All claims: {string.Join(", ", httpContext.User.Claims.Select(c => $"{c.Type}: {c.Value}"))}");

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            Console.WriteLine(" No valid user ID found in token");
            return Results.Unauthorized();
        }

  
        var firstName = httpContext.User.FindFirst("given_name")?.Value ?? "User";
        var lastName = httpContext.User.FindFirst("family_name")?.Value ?? "User";

        Console.WriteLine($" Creating profile for user: {userId}, Name: {firstName} {lastName}");

        var command = new UpdateProfileCommand(
            userId,
            firstName,
            lastName,
            null, 
            null, 
            null  
        );

        var result = await mediator.Send(command);

        if (result.IsSuccess)
        {
            Console.WriteLine($" Profile created successfully for user {userId}");
            return Results.Ok(new
            {
                message = "Profile initialized successfully",
                profileId = result.Data.ProfileId,
                userId = userId
            });
        }
        else
        {
            Console.WriteLine($" Profile creation failed: {string.Join(", ", result.Errors)}");
            return Results.BadRequest(result);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($" ERROR in init endpoint: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem($"Error initializing profile: {ex.Message}");
    }
}).RequireAuthorization();



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