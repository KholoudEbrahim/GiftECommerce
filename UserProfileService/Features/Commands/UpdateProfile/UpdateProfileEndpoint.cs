using FluentValidation;
using MediatR;
using UserProfileService.Features.Shared;
using UserProfileService.Infrastructure;

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
                Guid userId;
                try
                {
                    userId = httpContext.User.GetUserId();
                }
                catch
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
