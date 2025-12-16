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
        }
    }
}