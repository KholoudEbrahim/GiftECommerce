using CategoryService.Contracts;
using CategoryService.Contracts.Product.Images;
using CategoryService.shared.MarkerInterface;
using FluentValidation;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Products.Images
{
    public static class UploadProductImage
    {
        public sealed record Command(IFormFile File) : ICommand<Result<UploadProductImageResponse>>;



        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.File).NotNull().WithMessage("File is required.");

                RuleFor(x => x.File)
                    .Must(f => f.Length > 0)
                    .WithMessage("File cannot be empty.");

                // Optional: Validate Extension
                RuleFor(x => x.File)
                    .Must(f =>
                        f.ContentType.Equals("image/jpeg") ||
                        f.ContentType.Equals("image/png") ||
                        f.ContentType.Equals("image/webp"))
                    .WithMessage("Only JPEG, PNG, and WebP images are allowed.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<UploadProductImageResponse>>
        {
            private readonly IFileStorageService _fileService;

            public Handler(IFileStorageService fileService)
            {
                _fileService = fileService;
            }

            public async Task<Result<UploadProductImageResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                string relativePath = await _fileService.SaveFileAsync(request.File, "products");

                if (string.IsNullOrEmpty(relativePath))
                {
                    return Result.Failure<UploadProductImageResponse>(new Error("Upload.Failed", "Failed to upload image."));
                }


                string fullUrl = _fileService.GetFullUrl(relativePath);

                return Result.Success(new UploadProductImageResponse(fullUrl, relativePath));
            }
        }
    }
}
