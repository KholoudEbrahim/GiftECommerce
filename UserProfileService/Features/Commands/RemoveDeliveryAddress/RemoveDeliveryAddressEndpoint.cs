using FluentValidation;
using MediatR;
using UserProfileService.Features.Shared;
using UserProfileService.Infrastructure;

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
                Guid userId;
                try
                {
                    userId = httpContext.User.GetUserId();
                }
                catch
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