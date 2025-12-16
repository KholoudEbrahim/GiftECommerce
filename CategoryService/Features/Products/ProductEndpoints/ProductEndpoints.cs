using Carter;
using CategoryService.Contracts.Product;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CategoryService.Features.Products.ProductEndpoints
{
    public class ProductEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/products").WithTags("Products");

            // 1. CREATE PRODUCT
            group.MapPost("/", async ([FromBody] CreateProductRequest request, ISender sender) =>
            {
                var command = new CreateProduct.Command(
                    request.Name,
                    request.Description,
                    request.Price,
                    request.Discount,
                    request.CategoryId,
                    request.OccasionIds,
                    request.Tags,
                    request.ImageUrl,
                    request.IsActive
                );

                var result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Created($"/api/products/{result.Value}", new CreateProductResponse(result.Value))
                    : Results.BadRequest(result.Error);
            })
            .WithName("CreateProduct")
            .WithSummary("Creates a new product (US-A09)");


            // 2. UPDATE PRODUCT
            group.MapPut("/{id}", async (int id, [FromBody] UpdateProductRequest request, ISender sender) =>
            {
                if (id != request.Id)
                    return Results.BadRequest("Route ID does not match body ID");

                var command = new UpdateProduct.Command(
                    request.Id,
                    request.Name,
                    request.Description,
                    request.Price,
                    request.Discount,
                    request.CategoryId,
                    request.OccasionIds,
                    request.Tags,
                    request.ImageUrl,
                    request.IsActive
                );

                var result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(new { success = true })
                    : Results.BadRequest(result.Error);
            })
            .WithName("UpdateProduct")
            .WithSummary("Updates an existing product (US-A10)");


            // 3. DELETE PRODUCT
            group.MapDelete("/{id}", async (int id, ISender sender) =>
            {
                var command = new DeleteProduct.Command(id);
                var result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.NoContent()
                    : Results.BadRequest(result.Error);
            })
            .WithName("DeleteProduct")
            .WithSummary("Deletes a product (US-A11)");


            // 4. ACTIVATE PRODUCT
            group.MapPatch("/{id}/activate", async (int id, ISender sender) =>
            {
                var command = new ToggleProductStatus.Command(id, true);
                var result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(new { message = "Product activated successfully" })
                    : Results.BadRequest(result.Error);
            })
            .WithName("ActivateProduct")
            .WithSummary("Activates a product (US-A12)");

           
            // 5. DEACTIVATE PRODUCT
            group.MapPatch("/{id}/deactivate", async (int id, ISender sender) =>
            {
                var command = new ToggleProductStatus.Command(id, false);
                var result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(new { message = "Product deactivated successfully" })
                    : Results.BadRequest(result.Error);
            })
            .WithName("DeactivateProduct")
            .WithSummary("Deactivates a product (US-A12)");

            
            // 6. SEARCH PRODUCTS
            group.MapGet("/search", async (
                [FromQuery] string? searchTerm,
                [FromQuery] decimal? minPrice,
                [FromQuery] decimal? maxPrice,
                [FromQuery] int? categoryId,
                [FromQuery] int? occasionId,
                [FromQuery] List<string>? tags,
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                ISender sender) =>
            {
                var query = new SearchProducts.Query(
                    searchTerm,
                    minPrice,
                    maxPrice,
                    categoryId,
                    occasionId,
                    tags,
                    pageNumber == 0 ? 1 : pageNumber,
                    pageSize == 0 ? 20 : pageSize
                );

                var result = await sender.Send(query);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("SearchProducts")
            .WithSummary("Search products with filters (US-C04)");


            // 9. GET PRODUCT DETAILS
            group.MapGet("/{id}", async (int id, ISender sender) =>
            {
                var query = new GetProductDetails.Query(id);
                var result = await sender.Send(query);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.NotFound(result.Error);
            })
            .WithName("GetProductDetails")
            .WithSummary("Get detailed product information (US-C08)");




        }
    }
}