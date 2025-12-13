using Carter;
using CategoryService.Contracts.Occasion.Images;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Occasions.Images
{
    public class OccasionImageEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/images").WithTags("Images");

            // Upload Occasion Image
            group.MapPost("/occasions", async (IFormFile file, ISender sender) =>
            {
                var command = new UploadOccasionImage.Command(file);
                Result<UploadOccasionImageResponse> result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("UploadOccasionImage")
            .WithSummary("Uploads an image file for an occasion")
            .DisableAntiforgery();
        }
    }
}
