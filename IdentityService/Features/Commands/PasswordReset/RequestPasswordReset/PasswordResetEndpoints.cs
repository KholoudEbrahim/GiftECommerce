using IdentityService.Features.Commands.PasswordReset.ResendResetCode;
using IdentityService.Features.Commands.PasswordReset.VerifyResetCode;
using IdentityService.Features.Shared;
using MediatR;

namespace IdentityService.Features.Commands.PasswordReset.RequestPasswordReset
{
    public static class PasswordResetEndpoints
    {
        public static void MapPasswordResetEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/password/verify", async (
                VerifyResetCodeCommand command,
                IRequestHandler<VerifyResetCodeCommand, RequestResponse<VerifyResetCodeResponseDto>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(
                        EndpointResponse<VerifyResetCodeResponseDto>.Success(
                            result.Data!, result.Message, result.StatusCode))
                    : Results.BadRequest(
                        EndpointResponse<VerifyResetCodeResponseDto>.Fail(
                            result.Message, result.StatusCode));
            })
            .WithName("VerifyResetCode")
            .WithTags("Authentication")
            .Produces<EndpointResponse<VerifyResetCodeResponseDto>>(200)
            .Produces<EndpointResponse<VerifyResetCodeResponseDto>>(400)
            .Produces<EndpointResponse<VerifyResetCodeResponseDto>>(500);

            app.MapPost("/api/auth/password/resend", async (
                ResendResetCodeCommand command,
                IRequestHandler<ResendResetCodeCommand, RequestResponse<RequestPasswordResetResponseDto>> handler,
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
            .WithName("ResendResetCode")
            .WithTags("Authentication")
            .Produces<EndpointResponse<RequestPasswordResetResponseDto>>(200)
            .Produces<EndpointResponse<RequestPasswordResetResponseDto>>(400)
            .Produces<EndpointResponse<RequestPasswordResetResponseDto>>(500);
        }
    }
}
