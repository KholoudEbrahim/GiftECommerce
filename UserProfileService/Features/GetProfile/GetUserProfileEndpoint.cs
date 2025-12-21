using MediatR;
using UserProfileService.Features.Shared;

namespace UserProfileService.Features.GetProfile
{
    public static class GetUserProfileEndpoint
    {
        public static void MapGetUserProfileEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("api/profile", async (
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                // Get user ID from claims (from JWT token)
                var userIdClaim = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var query = new GetUserProfileQuery(userId);
                var result = await mediator.Send(query, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .RequireAuthorization()
            .WithName("GetUserProfile")
            .Produces<ApiResponse<UserProfileDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .WithTags("Profile");
        }
    }
}
