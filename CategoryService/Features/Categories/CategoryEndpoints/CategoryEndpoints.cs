using Carter;
using CategoryService.Contracts.Category;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Categories;

public class CategoryEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");


        // 1. Create Category
        group.MapPost("/", async ([FromBody] CreateCategoryRequest request, ISender sender) =>
        {
            var command = new CreateCategory.Command(request.Name, request.ImageUrl, request.IsActive);
            Result<int> result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/categories/{result.Value}", result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("CreateCategory")
        .WithSummary("Creates a new category");

        // 2. Get All Categories
        group.MapGet("/", async (ISender sender) =>
        {
            var query = new GetCategories.Query();
            Result<IEnumerable<GetCategoriesResponse>> result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetCategories")
        .WithSummary("Retrieves a list of all categories");

        // 3. Get Category Details (with Products)
        group.MapGet("/{id}", async (int id, ISender sender) =>
        {
            var query = new GetCategoryDetails.Query(id);
            Result<GetCategoryDetailsResponse> result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        })
        .WithName("GetCategoryDetails")
        .WithSummary("Retrieves detailed information about a specific category");

        // 4. Update Category
        group.MapPut("/{id}", async (int id, [FromBody] UpdateCategoryRequest request, ISender sender) =>
        {
            if (id != request.Id)
                return Results.BadRequest("Route ID does not match Body ID");

            var command = new UpdateCategory.Command(request.Id, request.Name, request.ImageUrl, request.IsActive);
            Result<bool> result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("UpdateCategory")
        .WithSummary("Updates an existing category");

        // 5. Delete Category
        group.MapDelete("/{id}", async (int id, ISender sender) =>
        {
            var command = new DeleteCategory.Command(id);
            Result<bool> result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("DeleteCategory")
        .WithSummary("Deletes a category if it has no dependencies");
    }
}