using FluentValidation;
using IdentityService.Data;
using IdentityService.Events;
using IdentityService.Features.Commands.Login;
using IdentityService.Features.Commands.PasswordReset.RequestPasswordReset;
using IdentityService.Features.Commands.PasswordReset.ResendResetCode;
using IdentityService.Features.Commands.PasswordReset.VerifyResetCode;
using IdentityService.Features.Commands.SignUp;
using IdentityService.Features.Shared;
using IdentityService.Services;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System;
using System.Text.Json;
using static IdentityService.Features.Commands.Login.LoginCommand;
using static IdentityService.Features.Commands.PasswordReset.ResendResetCode.ResetPasswordCommand;
using static IdentityService.Features.Commands.PasswordReset.VerifyResetCode.VerifyResetCodeCommand;
using static IdentityService.Features.Commands.SignUp.SignUpCommand;

namespace IdentityService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

           
            builder.Services.AddDbContext<IdentityDbContext>((services, options) =>
            {
                var configuration = services.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
                });
            });

      
            builder.Services.AddScoped<IRepository, Repository>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

         
            builder.Services.AddScoped<IValidator<SignUpCommand>, SignUpValidator>();
            builder.Services.AddScoped<IValidator<LoginCommand>, LoginValidator>();
            builder.Services.AddScoped<IValidator<VerifyResetCodeCommand>, VerifyResetCodeValidator>();
            builder.Services.AddScoped<IValidator<ResetPasswordCommand>, ResetPasswordValidator>();
            builder.Services.AddScoped<IValidator<RequestPasswordResetCommand>, RequestPasswordResetValidator>();

            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            builder.Services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
                    {
                        h.Username(builder.Configuration["RabbitMQ:Username"]);
                        h.Password(builder.Configuration["RabbitMQ:Password"]);
                    });
                });
            });

     
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

       
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Identity Service API",
                    Version = "v1",
                    Description = "Identity Service for Gift Ecommerce"
                });
            });

            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

          
            ApplyMigrationsWithRetry(app);

       
            app.MapRequestPasswordResetEndpoint();
            app.MapPasswordResetEndpoints();
            app.MapResetPasswordEndpoint();
            app.MapSignUpEndpoint();
            app.MapLoginEndpoint();

        
            app.MapGet("/health", () =>
            {
                return Results.Ok(new
                {
                    status = "Healthy",
                    service = "Identity Service",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    database = "Connected",
                    environment = app.Environment.EnvironmentName
                });
            })
            .WithName("HealthCheck")
            .WithTags("Health");

            app.UseAuthorization();
            app.Run();
        }

        private static void ApplyMigrationsWithRetry(WebApplication app)
        {
            var maxRetries = 10;
            var retryDelay = TimeSpan.FromSeconds(5);

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using (var scope = app.Services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

                      
                        if (dbContext.Database.CanConnect())
                        {
                            Console.WriteLine($"Database connection successful on attempt {i + 1}");

                          
                            var databaseName = dbContext.Database.GetDbConnection().Database;
                            var sql = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";
                            var databaseExists = dbContext.Database.ExecuteSqlRaw(sql) > 0;

                            if (!databaseExists)
                            {
                                Console.WriteLine($"Creating database: {databaseName}");
                                dbContext.Database.EnsureCreated();
                            }
                            else
                            {
                                Console.WriteLine($"Database {databaseName} already exists");
                            }

                          
                            Console.WriteLine("Applying migrations...");
                            dbContext.Database.Migrate();
                            Console.WriteLine("Migrations applied successfully!");
                            return;
                        }
                    }
                }
                catch (SqlException ex) when (i < maxRetries - 1)
                {
                    Console.WriteLine($"Database connection failed on attempt {i + 1}: {ex.Message}");
                    Console.WriteLine($"Retrying in {retryDelay.TotalSeconds} seconds...");
                    Thread.Sleep(retryDelay);

                   
                    retryDelay = TimeSpan.FromSeconds(retryDelay.TotalSeconds * 1.5);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    throw;
                }
            }

            Console.WriteLine("Failed to connect to database after all retries");
               throw new InvalidOperationException("Failed to connect to database");
        }
    }
}