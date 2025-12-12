using Carter;
using CategoryService.Contracts.Category.Images;
using CategoryService.Features.Categories.Images;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Images;

public class ImageEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/images").WithTags("Images");

        // Upload Category Image
        group.MapPost("/categories", async (IFormFile file, ISender sender) =>
        {
            var command = new UploadCategoryImage.Command(file);
            Result<UploadCategoryImageResponse> result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("UploadCategoryImage")
        .WithSummary("Uploads an image file for a category")
        .DisableAntiforgery(); // Important for file uploads
    }
}