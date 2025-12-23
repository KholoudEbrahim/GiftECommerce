using IdentityService.Features.Shared;
using MediatR;

namespace IdentityService.Features.Commands.PasswordReset.ResendResetCode
{
    public static class ResetPasswordEndpoint
    {
        public static void MapResetPasswordEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/password/reset", async (
                ResetPasswordCommand command,
                IRequestHandler<ResetPasswordCommand, RequestResponse<ResetPasswordResponseDto>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(
                        EndpointResponse<ResetPasswordResponseDto>.Success(
                            result.Data!, result.Message, result.StatusCode))
                    : Results.BadRequest(
                        EndpointResponse<ResetPasswordResponseDto>.Fail(
                            result.Message, result.StatusCode));
            })
            .WithName("ResetPassword")
            .WithTags("Authentication")
            .Produces<EndpointResponse<ResetPasswordResponseDto>>(200)
            .Produces<EndpointResponse<ResetPasswordResponseDto>>(400)
            .Produces<EndpointResponse<ResetPasswordResponseDto>>(500);
        }
    }
}
