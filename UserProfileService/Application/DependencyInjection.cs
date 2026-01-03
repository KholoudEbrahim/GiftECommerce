using FluentValidation;
using MediatR;
using System.Reflection;
using UserProfileService.Features.Shared;

namespace UserProfileService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

                cfg.Lifetime = ServiceLifetime.Scoped;
            });

            // FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Validation Behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}

