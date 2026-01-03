using IdentityService.Features.Shared;
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

                return result.StatusCode switch
                {
                    201 => Results.Created($"/api/users/{result.Data.UserId}",
                        EndpointResponse<SignUpResponseDto>.Success(result.Data, result.Message, result.StatusCode)),
                    400 => Results.BadRequest(
                        EndpointResponse<SignUpResponseDto>.Fail(result.Message, result.StatusCode)),
                    409 => Results.Conflict(
                        EndpointResponse<SignUpResponseDto>.Fail(result.Message, result.StatusCode)),
                    _ => Results.Problem(
                        detail: result.Message,
                        statusCode: result.StatusCode)
                };
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
