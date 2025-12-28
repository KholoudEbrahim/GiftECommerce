using Microsoft.Extensions.Caching.Memory;

namespace IdentityService.Middlewares
{
    public class BruteForceProtectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BruteForceProtectionMiddleware> _logger;

        public BruteForceProtectionMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<BruteForceProtectionMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
         
            if (context.Request.Path.StartsWithSegments("/api/auth"))
            {
                var ipAddress = GetClientIpAddress(context);

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    
                    var cacheKey = $"failed_attempts_{ipAddress}";
                    var failedAttempts = _cache.Get<int>(cacheKey);

                    if (failedAttempts >= 10)
                    {
                        _logger.LogWarning(
                            "IP {IpAddress} blocked due to too many failed authentication attempts. Count: {FailedAttempts}",
                            ipAddress, failedAttempts);

                        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Too many failed attempts. Please try again after 15 minutes.",
                            retryAfter = 900
                        });

                        return;
                    }
                }
            }

            await _next(context);
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

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }

    public static class FailedLoginTracker
    {
        private static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan ResetDuration = TimeSpan.FromMinutes(5);

        public static void RecordFailedAttempt(string ipAddress, IMemoryCache cache)
        {
            var cacheKey = $"failed_attempts_{ipAddress}";
            var currentAttempts = cache.Get<int>(cacheKey);

            cache.Set(cacheKey, currentAttempts + 1, new MemoryCacheEntryOptions
            {
                SlidingExpiration = ResetDuration,
                AbsoluteExpirationRelativeToNow = BlockDuration
            });
        }

        public static void ClearFailedAttempts(string ipAddress, IMemoryCache cache)
        {
            var cacheKey = $"failed_attempts_{ipAddress}";
            cache.Remove(cacheKey);
        }

        public static int GetFailedAttempts(string ipAddress, IMemoryCache cache)
        {
            var cacheKey = $"failed_attempts_{ipAddress}";
            return cache.Get<int>(cacheKey);
        }
    }
}