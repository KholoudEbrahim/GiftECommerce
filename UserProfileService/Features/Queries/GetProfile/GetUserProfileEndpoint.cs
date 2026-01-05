using FluentValidation;
using MediatR;
using UserProfileService.Features.Shared;
using UserProfileService.Infrastructure;

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
                Guid userId;
                try
                {
                    userId = httpContext.User.GetUserId();
                }
                catch
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
