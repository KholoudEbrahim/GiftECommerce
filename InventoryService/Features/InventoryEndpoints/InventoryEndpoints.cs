using Carter;
using InventoryService.Contracts.Stock;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Features.InventoryEndpoints
{
    public class InventoryEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/inventory").WithTags("Inventory");

            // 1. SET MIN/MAX STOCK
            group.MapPost("/min-max", async (
           [FromBody] SetMinMaxStockRequest request,
           ISender sender) =>
            {
                var command = new SetMinMaxStock.Command(
                    request.ProductId,
                    request.MinStock,
                    request.MaxStock
                );

                var result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(new { success = true, message = "Min/Max stock set successfully" })
                    : Results.BadRequest(result.Error);
            })
            .WithName("SetMinMaxStock")
            .WithSummary("Set minimum and maximum stock levels");


            // 2. ADD STOCK
            group.MapPost("/add", async (
            [FromBody] AddStockRequest request,
            ISender sender) =>
            {
                var command = new AddStock.Command(
                    request.ProductId,
                    request.Quantity,
                    request.Notes,
                    request.PerformedBy
                );

                var result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("AddStock")
            .WithSummary("Add stock quantity to a product");


            // 3. GET STOCK INFO
            group.MapGet("/{productId}", async (
           int productId,
           ISender sender) =>
            {
                var query = new GetStockInfo.Query(productId);
                var result = await sender.Send(query);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.NotFound(result.Error);
            })
            .WithName("GetStockInfo")
            .WithSummary("Get stock information for a specific product");


            // 4. GET STOCK TRANSACTIONS
            group.MapGet("/{productId}/transactions", async (
            int productId,
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender) =>
            {
                var query = new GetStockTransactions.Query(
                    productId,
                    pageNumber == 0 ? 1 : pageNumber,
                    pageSize == 0 ? 50 : pageSize
                );

                var result = await sender.Send(query);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("GetStockTransactions")
            .WithSummary("Get transaction history for a product");


            // 5. GET ALL STOCKS
            group.MapGet("/", async (
            [FromQuery] bool? onlyLowStock,
            ISender sender) =>
            {
                var query = new GetAllStocks.Query(onlyLowStock);
                var result = await sender.Send(query);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("GetAllStocks")
            .WithSummary("Get all stock records (optionally filter by low stock)");



            // 6. CHECK STOCK AVAILABILITY
            group.MapGet("/check/{productId}", async (
            int productId,
            [FromQuery] int quantity,
            ISender sender) =>
            {
                if (quantity <= 0)
                    return Results.BadRequest(new { error = "Quantity must be greater than 0" });

                var query = new CheckStockAvailability.Query(productId, quantity);
                var result = await sender.Send(query);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("CheckStockAvailability")
            .WithSummary("Check if requested quantity is available (used by CartService/CategoryService)")
            .WithDescription("Returns availability status and current stock level");


            
            // 7. BULK CHECK STOCK AVAILABILITY
            group.MapPost("/check-bulk", async (
                [FromBody] BulkCheckStockRequest request,
                ISender sender) =>
            {
                var products = request.Products
                    .Select(p => new BulkCheckStockAvailability.ProductQuantityRequest(p.ProductId, p.Quantity))
                    .ToList();

                var query = new BulkCheckStockAvailability.Query(products);
                var result = await sender.Send(query);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("BulkCheckStockAvailability")
            .WithSummary("Check stock availability for multiple products at once")
            .WithDescription("Used by CartService when checking entire cart");





        }


    }

    // Request DTO for bulk check
    public record BulkCheckStockRequest(
        List<ProductQuantityDto> Products
    );

    public record ProductQuantityDto(int ProductId, int Quantity);

}