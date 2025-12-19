namespace CartService.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                await _next(context);

                var elapsed = DateTime.UtcNow - startTime;

                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                var elapsed = DateTime.UtcNow - startTime;

                _logger.LogError(ex,
                    "HTTP {Method} {Path} threw exception after {ElapsedMilliseconds}ms: {ErrorMessage}",
                    context.Request.Method,
                    context.Request.Path,
                    elapsed.TotalMilliseconds,
                    ex.Message);

                throw;
            }
        }
    }
}
