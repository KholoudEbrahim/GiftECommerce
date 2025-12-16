using CategoryService.Contracts;
using CategoryService.Models;
using CategoryService.shared.MarkerInterface;
using Events.ProductEvents;
using FluentValidation;
using MassTransit;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Products
{
    public static class DeleteProduct
    {
        public sealed record Command(int Id) : ICommand<Result<bool>>;

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
                        new Error("DeleteProduct.Validation", validationResult.ToString()));

                // 2. Get Product
                var product = await _productRepo.GetByIdAsync(request.Id);
                if (product == null)
                    return Result.Failure<bool>(
                        new Error("Product.NotFound", $"Product with ID {request.Id} not found"));

                // 3. Check if product is in active orders (placeholder)

                // var hasActiveOrders = await _orderServiceClient.HasActiveOrders(request.Id);
                // if (hasActiveOrders)
                //     return Result.Failure<bool>(
                //         new Error("Product.HasActiveOrders", 
                //             "Cannot delete product because it's part of active orders"));

                // 4. Soft Delete
                await _productRepo.DeleteAsync(request.Id);
                await _productRepo.SaveChangesAsync(cancellationToken);

                // 5. 🐰 Publish Event
                await _publishEndpoint.Publish(new ProductDeletedEvent
                {
                    ProductId = product.Id,
                    DeletedAt = DateTime.UtcNow
                }, cancellationToken);

                return Result.Success(true);
            }
        }
    }
}

