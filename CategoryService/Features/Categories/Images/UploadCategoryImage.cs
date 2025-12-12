using CategoryService.Contracts;
using CategoryService.Contracts.Category.Images;
using CategoryService.shared.MarkerInterface;
using FluentValidation;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Categories.Images;


public static class UploadCategoryImage
{

    public sealed record Command(IFormFile File) : ICommand<Result<UploadCategoryImageResponse>>;

    

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

    internal sealed class Handler : IRequestHandler<Command, Result<UploadCategoryImageResponse>>
    {
        private readonly IFileStorageService _fileService;

        public Handler(IFileStorageService fileService)
        {
            _fileService = fileService;
        }

        public async Task<Result<UploadCategoryImageResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            string relativePath = await _fileService.SaveFileAsync(request.File, "categories");

            if (string.IsNullOrEmpty(relativePath))
            {
                return Result.Failure<UploadCategoryImageResponse>(new Error("Upload.Failed", "Failed to upload image."));
            }

            
            string fullUrl = _fileService.GetFullUrl(relativePath);
            
            return Result.Success(new UploadCategoryImageResponse(fullUrl, relativePath));
        }
    }
}