using Microsoft.EntityFrameworkCore;
using Polly;
using UserProfileService.Data;
using UserProfileService.Services;

namespace UserProfileService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database Context
            services.AddDbContext<UserProfileDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions =>
                    {
                        sqlServerOptions.MigrationsAssembly(typeof(UserProfileDbContext).Assembly.FullName);
                        sqlServerOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

            // Repository
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();

            // HTTP Client for Identity Service with Polly
            services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>((sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(config["IdentityService:BaseUrl"]);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
                .AddTransientHttpErrorPolicy(policy =>
                    policy.WaitAndRetryAsync(3, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .AddTransientHttpErrorPolicy(policy =>
                    policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

            return services;
        }
    }
}
