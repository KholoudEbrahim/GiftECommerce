using CartService.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Features.CartFeatures.Commands.AddCartItem
{
    public static class AddCartItemEndpoint
    {
        public static void MapAddCartItemEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/cart/items", async (
                [FromBody] AddCartItemRequest request,
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                CancellationToken cancellationToken) =>
            {

                Console.WriteLine($"UserId: {userContext.UserId}, AnonymousId: {userContext.AnonymousId}");

                var command = new AddCartItemCommand
                (
                    userContext.UserId,
                    userContext.AnonymousId,
                    request.ProductId,
                    request.Quantity
                );

                var result = await mediator.Send(command, cancellationToken);

                return Results.Created($"/api/cart/{result.CartId}/items/{result.ItemId}",
                    ApiResponse<AddCartItem.ResultDTO>.SuccessResponse(result));
            })
            .WithName("AddCartItem")
            .WithTags("Cart")
            .Produces<ApiResponse<AddCartItem.ResultDTO>>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .AllowAnonymous(); 
        }

        public record AddCartItemRequest
        {
            public required int ProductId { get; init; }
            public required int Quantity { get; init; }
        }
    }
}