using FluentValidation;
using MediatR;
using UserProfileService.Features.Shared;

namespace UserProfileService.Features.Queries.GetProfile
{
    public static class GetUserProfileEndpoint
    {
        public static void MapGetUserProfileEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("api/profile", async (
                HttpContext httpContext,
                IMediator mediator,
                IValidator<GetUserProfileQuery> validator,
                CancellationToken cancellationToken) =>
            {
           
                var userIdClaim = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var query = new GetUserProfileQuery(userId);

             
                var validationResult = await validator.ValidateAsync(query, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                    return Results.BadRequest(ApiResponse<object>.Failure(errors));
                }

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
