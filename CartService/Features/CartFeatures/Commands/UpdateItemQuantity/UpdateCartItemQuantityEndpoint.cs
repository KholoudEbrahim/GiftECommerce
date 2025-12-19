using CartService.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Features.CartFeatures.Commands.UpdateItemQuantity
{
    public static class UpdateItemQuantityEndpoint
    {
        public static void MapUpdateItemQuantityEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/cart/items/{productId}", async (
                [FromRoute] Guid productId,
                [FromBody] UpdateItemQuantityRequest request,
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateCartItemQuantityCommand(
                           userContext.UserId,
                      userContext.AnonymousId,
                         productId,
                       request.Quantity
                        );


                var result = await mediator.Send(command, cancellationToken);

                var response = ApiResponse<UpdateItemQuantity.CartItemQuantityDTO>.SuccessResponse(result);

                if (result.ItemRemoved)
                    return Results.Ok(response);

                return Results.Ok(response);
            })
            .WithName("UpdateItemQuantity")
            .WithTags("Cart")
            .Produces<ApiResponse<UpdateItemQuantity.CartItemQuantityDTO>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization("OptionalAuth");
        }

        public record UpdateItemQuantityRequest
        {
            public required int Quantity { get; init; }
        }
    }
}
