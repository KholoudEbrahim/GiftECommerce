using IdentityService.Shared;
using MediatR;

namespace IdentityService.Features.Commands.SignUp
{
    public static class SignUpEndpoint
    {
        public static void MapSignUpEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/signup", async (
                SignUpCommand command,
                IRequestHandler<SignUpCommand, RequestResponse<SignUpResponseDto>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/api/users/{result.Data.UserId}",
                        EndpointResponse<SignUpResponseDto>.Success(result.Data, result.Message, result.StatusCode))
                    : Results.BadRequest(
                        EndpointResponse<SignUpResponseDto>.Fail(result.Message, result.StatusCode));
            })
            .WithName("SignUp")
            .WithTags("Authentication")
            .Produces<EndpointResponse<SignUpResponseDto>>(201)
            .Produces<EndpointResponse<SignUpResponseDto>>(400)
            .Produces<EndpointResponse<SignUpResponseDto>>(409)
            .Produces<EndpointResponse<SignUpResponseDto>>(500);
        }
    }
}
