using FluentValidation;
using MediatR;
using UserProfileService.Features.Shared;

namespace UserProfileService.Features.Commands.UpdateProfile
{
    public static class UpdateProfileEndpoint
    {
        public static void MapUpdateProfileEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPut("api/profile", async (
                HttpContext httpContext,
                UpdateProfileRequest request,
                IMediator mediator,
                IValidator<UpdateProfileCommand> validator,
                CancellationToken cancellationToken) =>
            {
                // Get user ID from claims
                var userIdClaim = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new UpdateProfileCommand(
                    userId,
                    request.FirstName,
                    request.LastName,
                    request.PhoneNumber,
                    request.DateOfBirth,
                    request.ProfilePictureUrl);

                // Validate command
                var validationResult = await validator.ValidateAsync(command, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                    return Results.BadRequest(ApiResponse<object>.Failure(errors));
                }

                var result = await mediator.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .RequireAuthorization()
            .WithName("UpdateProfile")
            .Produces<ApiResponse<UpdatedProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .WithTags("Profile");


        }
    }
}
