using IdentityService.Features.Shared;
using MediatR;

namespace IdentityService.Features.Commands.ChangePassword
{
    public static class ChangePasswordEndpoint
    {
        public static void MapChangePasswordEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/users/{userId:guid}/password", async (
                Guid userId,
                ChangePasswordCommand command,
                IRequestHandler<ChangePasswordCommand, RequestResponse<ChangePasswordResponseDto>> handler,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                
                var authenticatedUserIdClaim = httpContext.User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(authenticatedUserIdClaim) ||
                    !Guid.TryParse(authenticatedUserIdClaim, out var authenticatedUserId) ||
                    authenticatedUserId != userId)
                {
                    return Results.Unauthorized();
                }

               
                var commandWithUserId = command with { UserId = userId };

                var result = await handler.Handle(commandWithUserId, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(
                        EndpointResponse<ChangePasswordResponseDto>.Success(
                            result.Data!,
                            result.Message,
                            result.StatusCode))
                    : Results.BadRequest(
                        EndpointResponse<ChangePasswordResponseDto>.Fail(
                            result.Message,
                            result.StatusCode));
            })
            .WithName("ChangePassword")
            .WithTags("User", "Security")
            .Produces<EndpointResponse<ChangePasswordResponseDto>>(200)
            .Produces<EndpointResponse<ChangePasswordResponseDto>>(400)
            .Produces<EndpointResponse<ChangePasswordResponseDto>>(401)
            .Produces<EndpointResponse<ChangePasswordResponseDto>>(404)
            .Produces<EndpointResponse<ChangePasswordResponseDto>>(500)
            .RequireAuthorization();
        }
    }
}
