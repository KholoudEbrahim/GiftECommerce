using IdentityService.Features.Shared;
using MediatR;

namespace IdentityService.Features.Commands.PasswordReset.RequestPasswordReset
{
    public static class RequestPasswordResetEndpoint
    {
        public static void MapRequestPasswordResetEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/password/forgot", async (
                RequestPasswordResetCommand command,
                IRequestHandler<RequestPasswordResetCommand, RequestResponse<RequestPasswordResetResponseDto>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(
                        EndpointResponse<RequestPasswordResetResponseDto>.Success(
                            result.Data!, result.Message, result.StatusCode))
                    : Results.BadRequest(
                        EndpointResponse<RequestPasswordResetResponseDto>.Fail(
                            result.Message, result.StatusCode));
            })
            .WithName("RequestPasswordReset")
            .WithTags("Authentication")
            .Produces<EndpointResponse<RequestPasswordResetResponseDto>>(200)
            .Produces<EndpointResponse<RequestPasswordResetResponseDto>>(400)
            .Produces<EndpointResponse<RequestPasswordResetResponseDto>>(500);
        }
    }
}
