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
            // MediatR
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Validation Behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // Auto-register all handlers
            services.Scan(scan => scan
                .FromAssemblyOf<Program>()
                .AddClasses()
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
    }
}
