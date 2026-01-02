using Carter;
using CartService.Data;
using CartService.Features.Shared;
using MediatR;

namespace CartService.Features.CartFeatures.CheckedByCategoryService
{
    public static class CartCheckEndpoints
    {

        public static void MapCartCheckEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/cart")
                .WithTags("Cart - Internal")
                .RequireAuthorization("ServiceAuth");

            group.MapGet("/reserved-quantity/{productId}", async (
                int productId,
                ICartRepository cartRepository,
                CancellationToken cancellationToken) =>
            {

                var cartsWithProduct = await cartRepository.GetCartsWithProductAsync(
                    productId,
                    cancellationToken);

                var reservedQuantity = cartsWithProduct
                    .SelectMany(c => c.Items)
                    .Where(i => i.ProductId == productId)
                    .Sum(i => i.Quantity);

                return Results.Ok(new { Quantity = reservedQuantity });
            })
            .WithName("GetReservedQuantity")
            .RequireAuthorization("ServiceAuth");


            group.MapGet("/check-product/{productId}", async (
                int productId,
                ICartRepository cartRepository,
                CancellationToken cancellationToken) =>
            {
                var cartsWithProduct = await cartRepository.GetCartsWithProductAsync(
                    productId,
                    cancellationToken);

                var isInCart = cartsWithProduct.Any();
                var totalCarts = cartsWithProduct.Count;
                var reservedQuantity = cartsWithProduct
                    .SelectMany(c => c.Items)
                    .Where(i => i.ProductId == productId)
                    .Sum(i => i.Quantity);

                return Results.Ok(new
                {
                    IsInCart = isInCart,
                    TotalCarts = totalCarts,
                    ReservedQuantity = reservedQuantity
                });
            })
            .WithName("CheckProductInCarts")
            .RequireAuthorization("ServiceAuth");
        }
    }

}
