using IdentityService.Features.Shared;
using MediatR;

namespace IdentityService.Features.Commands.Logout
{
    public static class LogoutEndpoint
    {
        public static void MapLogoutEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/logout", async (
                LogoutCommand command,
                IRequestHandler<LogoutCommand, RequestResponse<LogoutResponseDto>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(
                        EndpointResponse<LogoutResponseDto>.Success(
                            result.Data!,
                            result.Message,
                            result.StatusCode))
                    : Results.Json(
                        EndpointResponse<LogoutResponseDto>.Fail(
                            result.Message,
                            result.StatusCode),
                        statusCode: result.StatusCode);
            })
            .WithName("Logout")
            .WithTags("Authentication")
            .Produces<EndpointResponse<LogoutResponseDto>>(200)
            .Produces<EndpointResponse<LogoutResponseDto>>(400)
            .Produces<EndpointResponse<LogoutResponseDto>>(500);
        }
    }
}
