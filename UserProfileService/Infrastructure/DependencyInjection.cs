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


            services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>((sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var baseUrl = config["ExternalServices:IdentityServiceBaseUrl"];

                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException(
                        "ExternalServices:IdentityServiceBaseUrl is not configured");

                client.BaseAddress = new Uri(baseUrl);
            });


            return services;
        }
    }
}
