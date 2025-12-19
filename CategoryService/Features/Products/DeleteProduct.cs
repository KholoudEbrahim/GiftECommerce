using CategoryService.Contracts;
using CategoryService.Contracts.ExternalServices;
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
            private readonly ICartServiceClient _cartServiceClient;
            private readonly IOrderServiceClient _orderServiceClient;

            public Handler(
                IGenericRepository<Product, int> productRepo,
                IValidator<Command> validator,
                IPublishEndpoint publishEndpoint,
                ICartServiceClient cartServiceClient,
                IOrderServiceClient orderServiceClient)
            {
                _productRepo = productRepo;
                _validator = validator;
                _publishEndpoint = publishEndpoint;
                _cartServiceClient = cartServiceClient;
                _orderServiceClient = orderServiceClient;
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

                // 3. Check if product is in active orders

                var isInOrders = await _orderServiceClient.IsProductInActiveOrdersAsync(
               request.Id,
               cancellationToken);

                if (isInOrders)
                    return Result.Failure<bool>(
                        new Error("Product.InActiveOrders",
                            "Cannot delete product because it's part of active orders that are being processed or shipped."));

                // 4. Check if product is in active carts

                var isInCarts = await _cartServiceClient.IsProductInActiveCartsAsync(
                request.Id,
                cancellationToken);

                if (isInCarts)
                    return Result.Failure<bool>(
                        new Error("Product.InActiveCarts",
                            "Cannot delete product because it's in one or more active shopping carts. " +
                            "Please wait for customers to complete their purchases or remove it from their carts."));

                // 5. Soft Delete
                await _productRepo.DeleteAsync(request.Id);
                await _productRepo.SaveChangesAsync(cancellationToken);

                // 6. 🐰 Publish Event
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

