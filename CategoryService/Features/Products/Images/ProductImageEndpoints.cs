using Carter;
using CategoryService.Contracts.Product.Images;
using MediatR;

namespace CategoryService.Features.Products.Images
{
    public class ProductImageEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/images").WithTags("Images");

            // Upload Product Image
            group.MapPost("/products", async (IFormFile file, ISender sender) =>
            {
                
                var command = new UploadProductImage.Command(file);
                var result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(new UploadProductImageResponse(result.Value.ImageUrl, result.Value.RelativePath))
                    : Results.BadRequest(result.Error);
            })
            .WithName("UploadProductImage")
            .WithSummary("Upload a product image")
            .DisableAntiforgery();
        }
    }
}
