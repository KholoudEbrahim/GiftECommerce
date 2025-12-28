using AspNetCoreRateLimit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace IdentityService.Data.Configurations
{
    public static class RateLimitServiceConfiguration
    {
        public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
        
            var rateLimitConfig = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddJsonFile("appsettings.ratelimit.json", optional: true, reloadOnChange: true)
                .Build();

            services.AddMemoryCache();

         
            services.Configure<IpRateLimitOptions>(rateLimitConfig.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(rateLimitConfig.GetSection("IpRateLimitPolicies"));

            services.AddInMemoryRateLimiting();

            
            services.AddSingleton<IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();

            return services;
        }
    }
}