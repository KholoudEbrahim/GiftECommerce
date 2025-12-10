using IdentityService.Shared;
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
                var result = await handler.Handle(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(
                        EndpointResponse<LoginResponseDto>.Success(result.Data, result.Message, result.StatusCode))
                    : Results.Unauthorized();
            })
            .WithName("Login")
            .WithTags("Authentication")
            .Produces<EndpointResponse<LoginResponseDto>>(200)
            .Produces<EndpointResponse<LoginResponseDto>>(401)
            .Produces<EndpointResponse<LoginResponseDto>>(500);
        }
    }
}
