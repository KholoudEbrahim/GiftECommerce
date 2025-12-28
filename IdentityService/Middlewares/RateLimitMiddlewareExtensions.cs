using AspNetCoreRateLimit;

namespace IdentityService.Middlewares
{
    public static class RateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomRateLimiting(this IApplicationBuilder app)
        {
         

            app.UseMiddleware<CustomRateLimitResponseMiddleware>();
            app.UseMiddleware<BruteForceProtectionMiddleware>();
            app.UseIpRateLimiting();

            return app;
        }
    }
}
