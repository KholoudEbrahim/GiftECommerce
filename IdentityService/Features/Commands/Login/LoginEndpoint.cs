using IdentityService.Features.Shared;
using MediatR;

namespace IdentityService.Features.Commands.Login
{
    public static class LoginEndpoint
    {
     
        public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
        {
         
            app.MapPost("/api/auth/login", async (
                LoginCommand command,
                IRequestHandler<LoginCommand, RequestResponse<LoginResponseDto>> handler,
                CancellationToken cancellationToken) =>
            {
                // Default user login
                var result = await handler.Handle(command with { RequiredRole = null }, cancellationToken);
                return HandleLoginResult(result);
            })
            .WithName("UserLogin")
            .WithTags("Authentication");

            app.MapPost("/api/admin/auth/login", async (
                LoginCommand command,
                IRequestHandler<LoginCommand, RequestResponse<LoginResponseDto>> handler,
                CancellationToken cancellationToken) =>
            {
                // Admin only
                var result = await handler.Handle(command with { RequiredRole = "Admin" }, cancellationToken);
                return HandleLoginResult(result);
            })
            .WithName("AdminLogin")
            .WithTags("Authentication", "Admin");

          
        }

        private static IResult HandleLoginResult(RequestResponse<LoginResponseDto> result)
        {
            return result.IsSuccess
                ? Results.Ok(EndpointResponse<LoginResponseDto>.Success(result.Data!, result.Message, result.StatusCode))
                : Results.Json(
                    EndpointResponse<LoginResponseDto>.Fail(result.Message, result.StatusCode),
                    statusCode: result.StatusCode);
        }
    }
}
