using CartService.Data;
using CartService.Features.CartFeatures.Commands.AddCartItem;
using CartService.Features.CartFeatures.Commands.RemoveCartItem;
using CartService.Features.CartFeatures.Commands.SetDeliveryAddress;
using CartService.Features.CartFeatures.Commands.UpdateItemQuantity;
using CartService.Features.CartFeatures.Queries.GetCart;
using CartService.Features.Shared;
using CartService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Extensions
{
    public static class EndpointExtensions
    {
        public static void MapCartEndpoints(this IEndpointRouteBuilder app)
        {
            var cartEndpoints = app.MapGroup("/api/cart")
                .WithTags("Cart")
                .RequireAuthorization("OptionalAuth");

            // Map all endpoints
            AddCartItemEndpoint.MapAddCartItemEndpoint(app);
            GetCartEndpoint.MapGetCartEndpoint(app);
            UpdateItemQuantityEndpoint.MapUpdateItemQuantityEndpoint(app);
            RemoveItemEndpoint.MapRemoveItemEndpoint(app);
            SetDeliveryAddressEndpoint.MapSetDeliveryAddressEndpoint(app);
   

            // Additional utility endpoints
            cartEndpoints.MapDelete("", async (
                [FromServices] ICartRepository cartRepository,
                [FromServices] IUserContext userContext,
                CancellationToken cancellationToken) =>
            {
                Cart? cart = null;

                if (userContext.UserId.HasValue)
                {
                    cart = await cartRepository.GetActiveCartByUserIdAsync(userContext.UserId.Value, cancellationToken);
                }
                else if (!string.IsNullOrEmpty(userContext.AnonymousId))
                {
                    cart = await cartRepository.GetActiveCartByAnonymousIdAsync(userContext.AnonymousId, cancellationToken);
                }

                if (cart != null)
                {
                    cart.Clear();
                    await cartRepository.SaveChangesAsync(cancellationToken);
                }

                return Results.NoContent();
            })
            .WithName("ClearCart")
            .Produces(StatusCodes.Status204NoContent);
        }
    }
}
