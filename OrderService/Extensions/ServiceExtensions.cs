using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OrderService.Data;
using OrderService.Events.Publisher;
using OrderService.Features.Commands.PlaceOrder;
using OrderService.Features.Commands.RateOrderItem;
using OrderService.Features.Commands.ReOrder;
using OrderService.Features.Commands.VerifyCashPayment;
using OrderService.Features.Queries.GetOrderById;
using OrderService.Features.Queries.GetOrders;
using OrderService.Features.Queries.TrackOrder;
using OrderService.Features.Shared;
using OrderService.Features.Tracking;
using OrderService.Features.Tracking.TrackingStrategies;
using OrderService.Services;
using OrderService.Services.Cart;
using OrderService.Services.Inventory;
using OrderService.Services.Payment;
using OrderService.Services.TemporaryOrder;
using OrderService.Services.UserProfile;
using Stripe;
using System.Text.Json.Serialization;

namespace OrderService.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
           
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
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

            return services;
        }

        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("OrderDatabase"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });

            return services;
        }

        public static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["Redis:ConnectionString"];
                options.InstanceName = configuration["Redis:InstanceName"];
            });

            return services;
        }

        public static IServiceCollection AddMassTransitWithRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
                    var rabbitMqUsername = configuration["RabbitMQ:Username"] ?? "guest";
                    var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? "guest";

                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username(rabbitMqUsername);
                        h.Password(rabbitMqPassword);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }

        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<ICartServiceClient, CartServiceClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ExternalServicesSettings>>().Value;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutInSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "OrderService");
            })
            .AddPolicyHandler(HttpClientExtensions.GetRetryPolicy())
            .AddPolicyHandler(HttpClientExtensions.GetCircuitBreakerPolicy());

            services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ExternalServicesSettings>>().Value;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutInSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "OrderService");
            })
            .AddPolicyHandler(HttpClientExtensions.GetRetryPolicy())
            .AddPolicyHandler(HttpClientExtensions.GetCircuitBreakerPolicy());

            services.AddHttpClient<IProfileServiceClient, ProfileServiceClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ExternalServicesSettings>>().Value;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutInSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "OrderService");
            })
            .AddPolicyHandler(HttpClientExtensions.GetRetryPolicy())
            .AddPolicyHandler(HttpClientExtensions.GetCircuitBreakerPolicy());

            return services;
        }

        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["Jwt:Authority"];
                    options.Audience = configuration["Jwt:Audience"];
                    options.RequireHttpsMetadata = bool.Parse(configuration["Jwt:RequireHttpsMetadata"] ?? "false");

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Authority"],
                        ValidAudience = configuration["Jwt:Audience"]
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("sub");
                });
            });

            return services;
        }

        public static IServiceCollection AddMediatRServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });

            return services;
        }

        public static IServiceCollection AddFluentValidation(this IServiceCollection services)
        {
            services.AddScoped<IValidator<PlaceOrderCommand>, PlaceOrderCommandValidator>();
            services.AddScoped<IValidator<RateOrderItemCommand>, RateOrderItemCommandValidator>();
            services.AddScoped<IValidator<ReOrderCommand>, ReOrderCommandValidator>();
            services.AddScoped<IValidator<GetOrderByIdQuery>, GetOrderByIdQueryValidator>();
            services.AddScoped<IValidator<GetOrdersQuery>, GetOrdersQueryValidator>();
            services.AddScoped<IValidator<TrackOrderQuery>, TrackOrderQueryValidator>();
            services.AddScoped<IValidator<VerifyCashPaymentCommand>, VerifyCashPaymentCommandValidator>();

            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            return services;
        }

        public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddSqlServer(
                    configuration.GetConnectionString("OrderDatabase")!,
                    name: "order-db-check",
                    tags: new[] { "database", "sqlserver" })
                .AddRedis(
                    configuration["Redis:ConnectionString"] ?? "localhost:6379",
                    name: "redis-check",
                    tags: new[] { "cache", "redis" });

            return services;
        }

        public static IServiceCollection AddScopedServices(this IServiceCollection services)
        {
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IPaymentService, StripePaymentService>();
            services.AddScoped<ITrackingService, TrackingService>();
            services.AddScoped<ITrackingStrategyFactory, TrackingStrategyFactory>();
            services.AddScoped<ITemporaryOrderService, TemporaryOrderService>();
            services.AddScoped<IEventPublisher, EventPublisher>();

            // Register tracking strategies
            services.AddScoped<PendingTrackingStrategy>();
            services.AddScoped<ConfirmedTrackingStrategy>();
            services.AddScoped<ProcessingTrackingStrategy>();
            services.AddScoped<OutForDeliveryTrackingStrategy>();
            services.AddScoped<DeliveredTrackingStrategy>();

            services.AddHttpContextAccessor();

            return services;
        }

        public static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ExternalServicesSettings>(
                configuration.GetSection("ExternalServices"));
            services.Configure<StripeSettings>(
                configuration.GetSection("Stripe"));

            return services;
        }

        public static IServiceCollection AddStripeServices(this IServiceCollection services, IConfiguration configuration)
        {
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

            

            return services;
        }

        public static IServiceCollection AddProblemDetailsConfiguration(this IServiceCollection services)
        {
            services.AddProblemDetails();
            return services;
        }
    }
}
