
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace ApiGateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiAuthPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
            });
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy("UserRatePolicy", httpContext =>
            {
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                bool isAuthenticated = !string.IsNullOrEmpty(userId);

                string partitionKey = isAuthenticated
                    ? $"user:{userId}"
                    : $"ip:{httpContext.Connection.RemoteIpAddress}";

                int limit = isAuthenticated ? 100 : 50;

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    key => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limit,
                        Window = TimeSpan.FromSeconds(5),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });
        });

        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }



        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok(new
        {
            Status = "Gateway is operational",
            Time = DateTime.UtcNow
        }))
        .RequireRateLimiting("UserRatePolicy")
        .WithName("GetGatewayStatus");



        app.MapGet("/status/claims", (ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Results.Ok(new
            {
                UserId = userId,
                IsAuthenticated = user.Identity?.IsAuthenticated,
                Roles = roles,
                Message = "JWT token successfully validated by the Gateway."
            });
        })
        .RequireAuthorization()
        .RequireRateLimiting("UserRatePolicy")
        .WithName("GetAuthClaims");

        app.MapReverseProxy()
           .RequireRateLimiting("UserRatePolicy");

        app.Run();
    }
}
