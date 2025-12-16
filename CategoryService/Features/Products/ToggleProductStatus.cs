using CategoryService.Contracts;
using CategoryService.Models;
using CategoryService.Models.Enums;
using CategoryService.shared.MarkerInterface;
using Events.ProductEvents;
using FluentValidation;
using MassTransit;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Products
{
    public static class ToggleProductStatus
    {
        public sealed record Command(int Id, bool IsActive) : ICommand<Result<bool>>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Valid product ID is required");
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
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure<bool>(
                        new Error("ToggleProductStatus.Validation", validationResult.ToString()));

                // 2. Get Product
                var product = await _productRepo.GetByIdAsync(request.Id, trackChanges: true);
                if (product == null)
                    return Result.Failure<bool>(
                        new Error("Product.NotFound", $"Product with ID {request.Id} not found"));

                // 3. Update Status
                var newStatus = request.IsActive ? ProductStatus.InStock : ProductStatus.Unstock;

                // Only update if status actually changed
                if (product.Status == newStatus)
                    return Result.Success(true);

                product.Status = newStatus;
                product.UpdatedAtUtc = DateTime.UtcNow;

                string[] updatedProperties = [nameof(product.Status), nameof(product.UpdatedAtUtc)];
                _productRepo.SaveInclude(product, updatedProperties);
                await _productRepo.SaveChangesAsync(cancellationToken);

                // 4. 🐰 Publish Event
                await _publishEndpoint.Publish(new ProductStatusChangedEvent
                {
                    ProductId = product.Id,
                    Status = product.Status.ToString(),
                    ChangedAt = DateTime.UtcNow
                }, cancellationToken);

                return Result.Success(true);
            }
        }
    }
}

