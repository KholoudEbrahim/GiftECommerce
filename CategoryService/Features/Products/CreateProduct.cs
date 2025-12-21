using CategoryService.Contracts;
using CategoryService.Contracts.Product;
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
    public static class CreateProduct
    {
        public sealed record Command(
        string Name,
        string Description,
        decimal Price,
        decimal? Discount,
        int CategoryId,
        List<int> OccasionIds,
        List<string>? Tags,
        string? ImageUrl,
        bool IsActive
    ) : ICommand<Result<int>>;


        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Product name is required")
                    .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

                RuleFor(x => x.Description)
                    .NotEmpty().WithMessage("Description is required")
                    .MaximumLength(2000);

                RuleFor(x => x.Price)
                    .GreaterThan(0).WithMessage("Price must be greater than 0");

                RuleFor(x => x.Discount)
                    .GreaterThanOrEqualTo(0).When(x => x.Discount.HasValue)
                    .WithMessage("Discount cannot be negative");

                RuleFor(x => x.CategoryId)
                    .GreaterThan(0).WithMessage("Valid category is required");

                RuleFor(x => x.OccasionIds)
                    .NotEmpty().WithMessage("At least one occasion is required");
            }
        }


        internal sealed class Handler : IRequestHandler<Command, Result<int>>
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

            public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
            {
                // 1. Validation
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure<int>(
                        new Error("CreateProduct.Validation", validationResult.ToString()));

                // 2. Check if Category exists + Get Category Name
                var category = await _productRepo.ExecuteRawSqlAsync<CategoryDto>(
           "SELECT Id, Name FROM Categories WHERE Id = @p0",
           cancellationToken,
           request.CategoryId);

                if (category == null)
                    return Result.Failure<int>(
                        new Error("Category.NotFound", $"Category with ID {request.CategoryId} not found"));

                // 3. Check if all Occasions exist
                var occasionIds = request.OccasionIds.Distinct().ToList();

                if (occasionIds.Count == 0)
                    return Result.Failure<int>(
                        new Error("Occasion.Required", "At least one occasion is required"));

                var occasionIdsParam = string.Join(",", occasionIds);
                var existingOccasionCount = await _productRepo.ExecuteRawSqlScalarAsync<int>(
                    $"SELECT COUNT(DISTINCT Id) FROM Occasions WHERE Id IN ({occasionIdsParam})",
                    cancellationToken);

                if (existingOccasionCount != occasionIds.Count)
                    return Result.Failure<int>(
                        new Error("Occasion.NotFound", "One or more occasions not found"));

                // 4. Check for duplicate product name
                var nameExists = await _productRepo.AnyAsync(
                    p => p.Name == request.Name,
                    cancellationToken);

                if (nameExists)
                    return Result.Failure<int>(
                        new Error("Product.NameNotUnique",
                            $"Product with name '{request.Name}' already exists"));

                // 5. Create Product
                var product = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Discount = request.Discount,
                    CategoryId = request.CategoryId,
                    ImageUrl = request.ImageUrl,
                    Status = request.IsActive ? ProductStatus.InStock : ProductStatus.Unstock,
                    TagsList = request.Tags ?? new List<string>(),
                    CreatedAtUtc = DateTime.UtcNow
                };

                // 6. Add Product-Occasion relationships
                foreach (var occasionId in request.OccasionIds)
                {
                    product.ProductOccasions.Add(new ProductOccasion
                    {
                        OccasionId = occasionId
                    });
                }

                await _productRepo.AddAsync(product);
                await _productRepo.SaveChangesAsync(cancellationToken);

                // 7. 🐰 Publish Event to RabbitMQ
                await _publishEndpoint.Publish(new ProductCreatedEvent
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    CategoryId = product.CategoryId,
                    CategoryName = category.Name,
                    Price = product.Price,
                    CreatedAt = product.CreatedAtUtc
                }, cancellationToken);

                return Result.Success(product.Id);
            }
        }
    }
}
    

