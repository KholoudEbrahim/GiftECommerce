using CartService.Features.Shared;
using System.Text.Json;

namespace CartService.Middleware
{
    public class ResponseWrapperMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerOptions _jsonOptions;

        public ResponseWrapperMiddleware(RequestDelegate next)
        {
            _next = next;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
           
            if (ShouldSkipWrapping(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

      
            if (context.Response.StatusCode >= 400)
            {
         
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                return;
            }

           responseBody.Seek(0, SeekOrigin.Begin);
            var content = await new StreamReader(responseBody).ReadToEndAsync();

            object? data = null;
            if (!string.IsNullOrWhiteSpace(content) &&
                context.Response.ContentType?.Contains("application/json") == true)
            {
                try
                {
                    data = JsonSerializer.Deserialize<object>(content, _jsonOptions);
                }
                catch
                {
                    data = content;
                }
            }

            var wrappedResponse = new
            {
                Success = true,
                Data = data,
                Timestamp = DateTime.UtcNow,
                CorrelationId = context.Items["CorrelationId"] as string
            };

            var json = JsonSerializer.Serialize(wrappedResponse, _jsonOptions);


            context.Response.ContentType = "application/json";
            context.Response.ContentLength = null;

            using var newStream = new MemoryStream();
            using var writer = new StreamWriter(newStream);
            await writer.WriteAsync(json);
            await writer.FlushAsync();
            newStream.Seek(0, SeekOrigin.Begin);

            await newStream.CopyToAsync(originalBodyStream);
        }

        private static bool ShouldSkipWrapping(PathString path)
        {
            var skipPaths = new[]
            {
                "/health",
                "/swagger",
                "/favicon.ico",
                "/stripe-webhook"
            };

            return skipPaths.Any(p => path.StartsWithSegments(p));
        }
    }
}

