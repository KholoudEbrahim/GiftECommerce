
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OrderService.Data;
using OrderService.Extensions;
using OrderService.Features.Commands.PlaceOrder;
using OrderService.Features.Commands.RateOrderItem;
using OrderService.Features.Commands.ReOrder;
using OrderService.Features.Endpoints;
using OrderService.Features.Queries.GetOrderById;
using OrderService.Features.Queries.GetOrders;
using OrderService.Features.Queries.TrackOrder;
using OrderService.Features.Shared;
using OrderService.Features.Tracking;
using OrderService.Features.Tracking.TrackingStrategies;
using OrderService.Middleware;
using OrderService.Services;
using OrderService.Services.Cart;
using OrderService.Services.Inventory;
using OrderService.Services.Payment;
using OrderService.Services.UserProfile;
using Serilog;
using Stripe;
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

            // Add services
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });


            // Add tracking services
            builder.Services.AddScoped<ITrackingService, TrackingService>();
            builder.Services.AddScoped<ITrackingStrategyFactory, TrackingStrategyFactory>();

            // Register tracking strategies
            builder.Services.AddScoped<PendingTrackingStrategy>();
            builder.Services.AddScoped<ConfirmedTrackingStrategy>();
            builder.Services.AddScoped<ProcessingTrackingStrategy>();
            builder.Services.AddScoped<OutForDeliveryTrackingStrategy>();
            builder.Services.AddScoped<DeliveredTrackingStrategy>();

            // Add API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Order Service API",
                    Version = "v1",
                    Description = "Microservice for managing orders, payments, and deliveries"
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
            builder.Services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("OrderDatabase"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName);
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

            // Add MassTransit with RabbitMQ
            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
                    var rabbitMqUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
                    var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username(rabbitMqUsername);
                        h.Password(rabbitMqPassword);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            // Add HTTP clients for external services
            builder.Services.AddHttpClient<ICartServiceClient, CartServiceClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ExternalServicesSettings>>().Value;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutInSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "OrderService");
            })
            .AddPolicyHandler(HttpClientExtensions.GetRetryPolicy())
            .AddPolicyHandler(HttpClientExtensions.GetCircuitBreakerPolicy());

            builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ExternalServicesSettings>>().Value;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutInSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "OrderService");
            })
            .AddPolicyHandler(HttpClientExtensions.GetRetryPolicy())
            .AddPolicyHandler(HttpClientExtensions.GetCircuitBreakerPolicy());

            builder.Services.AddHttpClient<IProfileServiceClient, ProfileServiceClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ExternalServicesSettings>>().Value;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutInSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "OrderService");
            })
            .AddPolicyHandler(HttpClientExtensions.GetRetryPolicy())
            .AddPolicyHandler(HttpClientExtensions.GetCircuitBreakerPolicy());

            // Add authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = builder.Configuration["Jwt:Authority"];
                    options.Audience = builder.Configuration["Jwt:Audience"];
                    options.RequireHttpsMetadata = bool.Parse(builder.Configuration["Jwt:RequireHttpsMetadata"] ?? "false");

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Authority"],
                        ValidAudience = builder.Configuration["Jwt:Audience"]
                    };
                });

            // Add authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("sub");
                });
            });

            // Add MediatR
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });


            builder.Services.AddScoped<IValidator<PlaceOrderCommand>, PlaceOrderCommandValidator>();
            builder.Services.AddScoped<IValidator<RateOrderItemCommand>, RateOrderItemCommandValidator>();
            builder.Services.AddScoped<IValidator<ReOrderCommand>, ReOrderCommandValidator>();
            builder.Services.AddScoped<IValidator<GetOrderByIdQuery>, GetOrderByIdQueryValidator>();
            builder.Services.AddScoped<IValidator<GetOrdersQuery>, GetOrdersQueryValidator>();
            builder.Services.AddScoped<IValidator<TrackOrderQuery>, TrackOrderQueryValidator>();

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

           
            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    builder.Configuration.GetConnectionString("OrderDatabase")!,
                    name: "order-db-check",
                    tags: new[] { "database", "sqlserver" })
                .AddRedis(
                    builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379",
                    name: "redis-check",
                    tags: new[] { "cache", "redis" });

            // Add scoped services
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IUserContext, UserContext>();
            builder.Services.AddScoped<IPaymentService, StripePaymentService>();
            builder.Services.AddHttpContextAccessor();

            // Add configuration
            builder.Services.Configure<ExternalServicesSettings>(
                builder.Configuration.GetSection("ExternalServices"));
            builder.Services.Configure<StripeSettings>(
                builder.Configuration.GetSection("Stripe"));

            // Add problem details
            builder.Services.AddProblemDetails();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Apply migrations
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                await dbContext.Database.MigrateAsync();
            }

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
            app.MapOrderEndpoints();
            app.MapHealthChecks("/health");

         
            app.MapPost("/stripe-webhook", async (HttpContext context) =>
            {
                var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
                var stripeSignature = context.Request.Headers["Stripe-Signature"];

                var stripeSettings = context.RequestServices.GetRequiredService<IOptions<StripeSettings>>().Value;
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                try
                {
                    var stripeEvent = EventUtility.ConstructEvent(
                        json,
                        stripeSignature,
                        stripeSettings.WebhookSecret
                    );

                    // Handle different event types
                    switch (stripeEvent.Type)
                    {
                        case "payment_intent.succeeded":
                            var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
                            logger.LogInformation("Payment succeeded for payment intent: {PaymentIntentId}",
                                paymentIntent?.Id);
                            break;

                        case "payment_intent.payment_failed":
                            var failedPaymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
                            logger.LogWarning("Payment failed for payment intent: {PaymentIntentId}",
                                failedPaymentIntent?.Id);
                            break;

                        case "charge.refunded":
                            var charge = stripeEvent.Data.Object as Stripe.Charge;
                            logger.LogInformation("Charge refunded: {ChargeId}",
                                charge?.Id);
                            break;

                        default:
                            logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                            break;
                    }

                    return Results.Ok();
                }
                catch (StripeException ex)
                {
                    logger.LogError(ex, "Stripe webhook error");
                    return Results.BadRequest();
                }
            })
            .AllowAnonymous()
            .WithTags("Webhooks");

            await app.RunAsync();
        }
    }
}
