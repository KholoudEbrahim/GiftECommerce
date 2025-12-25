using FluentValidation;
using MediatR;
using UserProfileService.Features.Shared;

namespace UserProfileService.Features.Commands.RemoveDeliveryAddress
{
    public static class RemoveDeliveryAddressEndpoint
    {
        public static void MapRemoveDeliveryAddressEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapDelete("api/profile/addresses/{addressId}", async (
                HttpContext httpContext,
                IMediator mediator,
                Guid addressId,
                CancellationToken cancellationToken) =>
            {
             
                var userIdClaim = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new RemoveDeliveryAddressCommand(userId, addressId);

                var result = await mediator.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .RequireAuthorization()
            .WithName("RemoveDeliveryAddress")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .WithTags("Addresses");
        }
    }
}
