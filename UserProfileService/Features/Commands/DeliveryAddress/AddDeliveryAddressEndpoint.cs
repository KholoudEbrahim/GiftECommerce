using FluentValidation;
using MediatR;
using UserProfileService.Features.Shared;

namespace UserProfileService.Features.Commands.DeliveryAddress
{
    public static class AddDeliveryAddressEndpoint
    {
        public static void MapAddDeliveryAddressEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("api/profile/addresses", async (
                HttpContext httpContext,
                AddDeliveryAddressRequest request,
                IMediator mediator,
                IValidator<AddDeliveryAddressCommand> validator,
                CancellationToken cancellationToken) =>
            {
                // Get user ID from claims
                var userIdClaim = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new AddDeliveryAddressCommand(
                    userId,
                    request.Alias,
                    request.Street,
                    request.City,
                    request.Governorate,
                    request.Building,
                    request.Floor,
                    request.Apartment,
                    request.IsPrimary);

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
            .WithName("AddDeliveryAddress")
            .Produces<ApiResponse<DeliveryAddressResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .WithTags("Profile");
        }
    }
}
