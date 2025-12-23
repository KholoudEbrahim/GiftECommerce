using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using Microsoft.OpenApi.Models;
using IdentityService.Data;
using IdentityService.Services;
using IdentityService.Features.Commands.SignUp;
using IdentityService.Features.Commands.Login;
using IdentityService.Features.Commands.PasswordReset;
using MassTransit;

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
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
                        ?? "Server=sqlserver;Database=IdentityDb;User Id=sa;Password=Strong!Passw0rd123;TrustServerCertificate=True"));

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

                // MediatR
                builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

                // ? FIXED: RabbitMQ uses "rabbit" not "localhost"
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

                // ? FIXED: Add Authentication
                builder.Services.AddAuthentication();
                builder.Services.AddAuthorization();

                // Swagger
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo 
                    { Title = "Identity Service API", Version = "v1" }));

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

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

                app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Identity Service" }));

                // ? FIXED: Authentication middleware
                app.UseAuthentication();
                app.UseAuthorization();

                Console.WriteLine("? Starting on port 8080...");
                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERROR: {ex.Message}");
                Thread.Sleep(10000);
                throw;
            }
        }
    }
}
