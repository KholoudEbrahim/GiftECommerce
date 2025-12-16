using CategoryService.Contracts;
using CategoryService.Models;
using CategoryService.Models.Enums;
using CategoryService.shared.MarkerInterface;
using Events.ProductEvents;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Products
{
    public static class UpdateProduct
    {
        public sealed record Command(
        int Id,
        string Name,
        string Description,
        decimal Price,
        decimal? Discount,
        int CategoryId,
        List<int> OccasionIds,
        List<string>? Tags,
        string? ImageUrl,
        bool IsActive
    ) : ICommand<Result<bool>>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Valid product ID is required");

                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Product name is required")
                    .MaximumLength(200);

                RuleFor(x => x.Description)
                    .NotEmpty().WithMessage("Description is required")
                    .MaximumLength(2000);

                RuleFor(x => x.Price)
                    .GreaterThan(0).WithMessage("Price must be greater than 0");

                RuleFor(x => x.CategoryId)
                    .GreaterThan(0).WithMessage("Valid category is required");

                RuleFor(x => x.OccasionIds)
                    .NotEmpty().WithMessage("At least one occasion is required");
            }
        }


        internal sealed class Handler : IRequestHandler<Command, Result<bool>>
        {
            private readonly IGenericRepository<Product, int> _productRepo;
            private readonly IValidator<Command> _validator;
            private readonly IPublishEndpoint _publishEndpoint;

            public Handler(
                IGenericRepository<Product, int> productRepo,
                IValidator<Command> validator,
                IPublishEndpoint publishEndpoint)
            {
                _productRepo = productRepo;
                _validator = validator;
                _publishEndpoint = publishEndpoint;
            }

            public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
            {
                // 1. Validation
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return Result.Failure<bool>(
                        new Error("UpdateProduct.Validation", validationResult.ToString()));

                // 2. Single Query: Get Product with Occasions (with tracking)
                var product = await _productRepo
                    .GetAll(p => p.Id == request.Id, trackChanges: true)
                    .Include(p => p.ProductOccasions)
                    .FirstOrDefaultAsync(cancellationToken);

                if (product == null)
                    return Result.Failure<bool>(
                        new Error("Product.NotFound", $"Product with ID {request.Id} not found"));

                // 3. Single Query: Check Category exists
                var categoryExists = await _productRepo.ExecuteRawSqlScalarAsync<int>(
                    "SELECT COUNT(*) FROM Categories WHERE Id = @p0",
                    cancellationToken,
                    request.CategoryId);

                if (categoryExists == 0)
                    return Result.Failure<bool>(
                        new Error("Category.NotFound", $"Category with ID {request.CategoryId} not found"));

                // 4. Single Query: Check all Occasions exist
                var occasionIds = request.OccasionIds.Distinct().ToList();

                if (occasionIds.Count == 0)
                    return Result.Failure<bool>(
                        new Error("Occasion.Required", "At least one occasion is required"));

                var occasionIdsParam = string.Join(",", occasionIds);
                var existingOccasionCount = await _productRepo.ExecuteRawSqlScalarAsync<int>(
                    $"SELECT COUNT(DISTINCT Id) FROM Occasions WHERE Id IN ({occasionIdsParam})",
                    cancellationToken);

                if (existingOccasionCount != occasionIds.Count)
                    return Result.Failure<bool>(
                        new Error("Occasion.NotFound", "One or more occasions not found"));

                // 5. Single Query: Check for duplicate name (excluding current product)
                var nameExists = await _productRepo.AnyAsync(
                    p => p.Name == request.Name && p.Id != request.Id,
                    cancellationToken);

                if (nameExists)
                    return Result.Failure<bool>(
                        new Error("Product.NameNotUnique",
                            $"Another product with name '{request.Name}' already exists"));

                // 6. Update Product Properties
                product.Name = request.Name;
                product.Description = request.Description;
                product.Price = request.Price;
                product.Discount = request.Discount;
                product.CategoryId = request.CategoryId;
                product.ImageUrl = request.ImageUrl;
                product.Status = request.IsActive ? ProductStatus.InStock : ProductStatus.Unstock;
                product.TagsList = request.Tags ?? new List<string>();
                product.UpdatedAtUtc = DateTime.UtcNow;

                // 7. Update Occasions (Remove old, Add new)
                product.ProductOccasions.Clear();
                foreach (var occasionId in occasionIds)
                {
                    product.ProductOccasions.Add(new ProductOccasion
                    {
                        ProductId = product.Id,
                        OccasionId = occasionId
                    });
                }

                // 8. Save Changes
                _productRepo.Update(product);
                await _productRepo.SaveChangesAsync(cancellationToken);

                // 9. 🐰 Publish Event
                await _publishEndpoint.Publish(new ProductUpdatedEvent
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    IsActive = product.Status == ProductStatus.InStock,
                    UpdatedAt = product.UpdatedAtUtc.Value
                }, cancellationToken);

                return Result.Success(true);
            }
        }
    }
}
