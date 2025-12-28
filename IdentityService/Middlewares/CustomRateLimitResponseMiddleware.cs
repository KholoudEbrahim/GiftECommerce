using System.Text.Json;

namespace IdentityService.Middlewares
{
    public class CustomRateLimitResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomRateLimitResponseMiddleware> _logger;

        public CustomRateLimitResponseMiddleware(
            RequestDelegate next,
            ILogger<CustomRateLimitResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

          
            if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
            {
            
                var ipAddress = GetClientIpAddress(context);
                var endpoint = context.Request.Path;

                _logger.LogWarning(
                    "Rate limit hit for IP {IpAddress} on endpoint {Endpoint}",
                    ipAddress, endpoint);

               context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Too many requests",
                    message = "Please slow down and try again later.",
                    retryAfter = context.Response.Headers.ContainsKey("Retry-After")
                        ? context.Response.Headers["Retry-After"].ToString()
                        : "60",
                    endpoint = endpoint,
                    timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }

        private string GetClientIpAddress(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                return forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            }

            if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
            {
                return realIp.FirstOrDefault();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}