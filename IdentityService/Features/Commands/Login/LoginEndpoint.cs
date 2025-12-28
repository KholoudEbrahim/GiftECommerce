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
                var result = await handler.Handle(command, cancellationToken);
                return HandleLoginResult(result);
            })
            .WithName("Login")
            .WithTags("Authentication")
            .Produces<EndpointResponse<LoginResponseDto>>(200)
            .Produces<EndpointResponse<LoginResponseDto>>(400)
            .Produces<EndpointResponse<LoginResponseDto>>(401)
            .Produces<EndpointResponse<LoginResponseDto>>(429)
            .Produces<EndpointResponse<LoginResponseDto>>(500);
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
