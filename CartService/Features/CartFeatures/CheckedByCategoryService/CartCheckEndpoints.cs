using Carter;
using CartService.Features.Shared;
using MediatR;

namespace CartService.Features.CartFeatures.CheckedByCategoryService
{
    public class CartCheckEndpoints : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/cart")
                .WithTags("Cart - Internal")
                .WithDescription("Internal endpoints for inter-service communication");
        }

        //   // 1. CHECK IF PRODUCT IN ACTIVE CARTS
        //    group.MapGet("/check-product/{productId}", async (
        //        int productId,
        //        ISender sender) =>
        //    {
        //        var query = new CheckProductInActiveCarts.Query(productId);
        //        var result = await sender.Send(query);

        //        return result.IsSuccess
        //            ? Results.Ok(result.Value)
        //            : Results.BadRequest(result.Error);
        //    })
        //    .WithName("CheckProductInActiveCarts")
        //    .WithSummary("Check if product exists in any active shopping cart")
        //    .WithDescription("Used by CategoryService before deleting a product");

        //    // 2. GET RESERVED QUANTITY FOR A PRODUCT
        //    group.MapGet("/reserved-quantity/{productId}", async (
        //    int productId,
        //    ISender sender) =>
        //    {
        //        var query = new GetReservedQuantity.Query(productId);
        //        var result = await sender.Send(query);

        //        return result.IsSuccess
        //            ? Results.Ok(result.Value)
        //            : Results.BadRequest(result.Error);
        //    })
        //.WithName("GetReservedQuantityForProduct")
        //.WithSummary("Get total reserved quantity for a product across all carts")
        //.WithDescription("Used by InventoryService to calculate available stock");


        public record CheckProductInCartsResponse(
               bool IsInCart,
               int TotalCarts,
               int ReservedQuantity
        );

        public record GetReservedQuantityResponse(
              int ProductId,
              int Quantity
        );


    }
}
