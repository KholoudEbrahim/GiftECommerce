using AspNetCoreRateLimit;
using FluentValidation;
using IdentityService.Data;
using IdentityService.Data.Configurations;
using IdentityService.Events;
using IdentityService.Features.Commands.ChangePassword;
using IdentityService.Features.Commands.Login;
using IdentityService.Features.Commands.Logout;
using IdentityService.Features.Commands.PasswordReset.RequestPasswordReset;
using IdentityService.Features.Commands.PasswordReset.ResendResetCode;
using IdentityService.Features.Commands.PasswordReset.VerifyResetCode;
using IdentityService.Features.Commands.SignUp;
using IdentityService.Features.Shared;
using IdentityService.Middlewares;
using IdentityService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace IdentityService
{
      public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("?? Starting Identity Service...");
            
            try
            {
                var builder = WebApplication.CreateBuilder(args);


                // Database
                builder.Services.AddDbContext<IdentityDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


                // Services
                builder.Services.AddScoped<IRepository, Repository>();
                builder.Services.AddScoped<IPasswordService, PasswordService>();
                builder.Services.AddScoped<ITokenService, TokenService>();
                builder.Services.AddScoped<IEmailService, EmailService>();

                // Validators
                builder.Services.AddScoped<IValidator<SignUpCommand>, SignUpValidator>();
                builder.Services.AddScoped<IValidator<LoginCommand>, LoginValidator>();
                builder.Services.AddScoped<IValidator<VerifyResetCodeCommand>, VerifyResetCodeValidator>();
                builder.Services.AddScoped<IValidator<ResetPasswordCommand>, ResetPasswordValidator>();
                builder.Services.AddScoped<IValidator<RequestPasswordResetCommand>, RequestPasswordResetValidator>();
                builder.Services.AddScoped<IValidator<LogoutCommand>, LogoutValidator>();
                builder.Services.AddScoped<IValidator<ChangePasswordCommand>, ChangePasswordValidator>();

                // MediatR
                builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

              
                builder.Services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("rabbit", "/", h =>
                        {
                            h.Username("admin");
                            h.Password("admin123");
                        });
                    });
                });

          
                builder.Services.AddAuthentication();
                builder.Services.AddAuthorization();


                builder.Services.AddRateLimiting(builder.Configuration);
                builder.Services.AddHttpContextAccessor();
                // Swagger
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo 
                    { Title = "Identity Service API", Version = "v1" }));

                var app = builder.Build();


                app.UseMiddleware<CustomRateLimitResponseMiddleware>();
                app.UseMiddleware<BruteForceProtectionMiddleware>();
                app.UseIpRateLimiting();

           
                    app.UseSwagger();
                    app.UseSwaggerUI();
              

                app.UseHttpsRedirection();

                // Simple DB check
                try
                {
                    using var scope = app.Services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                    if (db.Database.CanConnect()) Console.WriteLine("? Database connected");
                }
                catch { }

                // Endpoints
                app.MapRequestPasswordResetEndpoint();
                app.MapPasswordResetEndpoints();
                app.MapResetPasswordEndpoint();
                app.MapSignUpEndpoint();
                app.MapLoginEndpoint();
                app.MapLogoutEndpoint();
                app.MapChangePasswordEndpoint();

                app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Identity Service" }));

               
                app.UseAuthentication();
                app.UseAuthorization();

                Console.WriteLine("? Starting on port 8080...");
                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERROR: {ex.Message}");
                throw;
            }
        }
    }

}