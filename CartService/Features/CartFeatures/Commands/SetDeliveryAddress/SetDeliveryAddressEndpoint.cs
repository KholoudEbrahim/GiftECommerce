using CartService.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Features.CartFeatures.Commands.SetDeliveryAddress
{
    public static class SetDeliveryAddressEndpoint
    {
        public static void MapSetDeliveryAddressEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/cart/delivery-address", async (
                [FromBody] SetDeliveryAddressRequest request,
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                CancellationToken cancellationToken) =>
            {
                var command = new SetDeliveryAddressCommand(
                      userContext.UserId,
                      userContext.AnonymousId,
                        request.AddressId
                          );


                var result = await mediator.Send(command, cancellationToken);

                return Results.Ok(ApiResponse<SetDeliveryAddress.SelectDeliveryAddressDTO>.SuccessResponse(result));
            })
            .WithName("SetDeliveryAddress")
            .WithTags("Cart")
            .Produces<ApiResponse<SetDeliveryAddress.SelectDeliveryAddressDTO>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization("OptionalAuth");
        }

        public record SetDeliveryAddressRequest
        {
            public required Guid AddressId { get; init; }
        }
    }
}
