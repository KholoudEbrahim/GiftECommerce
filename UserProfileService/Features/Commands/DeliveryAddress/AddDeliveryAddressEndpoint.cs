using FluentValidation;
using MediatR;
using UserProfileService.Features.Shared;
using UserProfileService.Infrastructure;

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
                Guid userId;
                try
                {
                    userId = httpContext.User.GetUserId();
                }
                catch
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
