using Azure.Core;
using CartService.Features.CartFeatures.Commands.SetDeliveryAddress;
using CartService.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Features.CartFeatures.Commands.RemoveCartItem
{
    public static class RemoveItemEndpoint
    {
        public static void MapRemoveItemEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/cart/items/{productId}", async (
                [FromRoute] Guid productId,
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                CancellationToken cancellationToken) =>
            {
                var command = new RemoveCartItemCommand(
                      userContext.UserId,
                  userContext.AnonymousId,
                         productId
                      );


                var result = await mediator.Send(command, cancellationToken);

                return Results.Ok(ApiResponse<RemoveCartItemDTO>.SuccessResponse(result));
            })
            .WithName("RemoveItem")
            .WithTags("Cart")
            .Produces<ApiResponse<RemoveCartItemDTO>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization("OptionalAuth");
        }
    }
}
