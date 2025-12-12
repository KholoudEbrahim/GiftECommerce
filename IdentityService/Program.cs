using FluentValidation;
using IdentityService.Data;
using IdentityService.Features.Commands.Login;
using IdentityService.Features.Commands.PasswordReset;
using IdentityService.Features.Commands.SignUp;
using IdentityService.Features.Shared;
using IdentityService.Services;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using static IdentityService.Features.Commands.Login.LoginCommand;
using static IdentityService.Features.Commands.PasswordReset.ResetPasswordCommand;
using static IdentityService.Features.Commands.PasswordReset.VerifyResetCodeCommand;

namespace IdentityService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            Console.WriteLine("=== DOCKER START ===");
            Console.WriteLine($"In Docker: {Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")}");
            Console.WriteLine($"URLS: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");


     

            builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    
            builder.Services.AddScoped<IRepository, Repository>();

 
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IEmailService, EmailService>();


            builder.Services.AddScoped<IValidator<SignUpCommand>, SignUpValidator>();
            builder.Services.AddScoped<IValidator<LoginCommand>, LoginValidator>();
            builder.Services.AddScoped<IValidator<VerifyResetCodeCommand>, VerifyResetCodeValidator>();
            builder.Services.AddScoped<IValidator<ResetPasswordCommand>, ResetPasswordValidator>();


            builder.Services.AddScoped<IRequestHandler<RequestPasswordResetCommand, RequestResponse<RequestPasswordResetResponseDto>>, RequestPasswordResetCommandHandler>();
            builder.Services.AddScoped<IRequestHandler<VerifyResetCodeCommand, RequestResponse<VerifyResetCodeResponseDto>>, VerifyResetCodeCommandHandler>();
            builder.Services.AddScoped<IRequestHandler<ResendResetCodeCommand, RequestResponse<RequestPasswordResetResponseDto>>, ResendResetCodeCommandHandler>();
            builder.Services.AddScoped<IRequestHandler<ResetPasswordCommand, RequestResponse<ResetPasswordResponseDto>>, ResetPasswordCommandHandler>();
            builder.Services.AddScoped<IRequestHandler<SignUpCommand, RequestResponse<SignUpResponseDto>>, SignUpCommandHandler>();
            builder.Services.AddScoped<IRequestHandler<LoginCommand, RequestResponse<LoginResponseDto>>, LoginCommandHandler>();


            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

            builder.Services.AddMediatR(cfg =>
              cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                dbContext.Database.Migrate(); 
            }

            app.MapRequestPasswordResetEndpoint();
            app.MapPasswordResetEndpoints();
            app.MapResetPasswordEndpoint();
            app.MapSignUpEndpoint();
            app.MapLoginEndpoint();

            app.MapGet("/health", () =>
            {
               
                var port = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(':').LastOrDefault() ?? "8080";

                return Results.Ok(new
                {
                    status = "Healthy",
                    service = "Identity Service",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    port = port,  
                    externalAccess = "http://localhost:5001",
                    internalPort = 8080
                });
            })
            .WithName("HealthCheck")
            .WithTags("Health");


            app.UseAuthorization();

            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast")
            .WithOpenApi();

            app.Run();
        }
    }
}