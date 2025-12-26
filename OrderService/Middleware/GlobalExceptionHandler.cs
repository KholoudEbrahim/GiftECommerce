using Microsoft.AspNetCore.Diagnostics;
using OrderService.Features.Shared;
using System.Net;

namespace OrderService.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var statusCode = GetStatusCode(exception);
            var response = new ApiErrorResponse
            {
                StatusCode = statusCode,
                Message = GetMessage(exception),
                Details = _environment.IsDevelopment() ? exception.ToString() : null,
                CorrelationId = httpContext.Items["CorrelationId"] as string
            };

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                InvalidOperationException => (int)HttpStatusCode.Conflict,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        private static string GetMessage(Exception exception)
        {
            return exception switch
            {
                KeyNotFoundException => "The requested resource was not found.",
                ArgumentException => "Invalid request parameters.",
                InvalidOperationException => "Operation cannot be performed.",
                UnauthorizedAccessException => "Access denied.",
                _ => "An unexpected error occurred."
            };
        }
    }
}
