using CategoryService.Contracts;
using CategoryService.Models;
using CategoryService.shared.MarkerInterface;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Products
{
    public static class IncrementProductViewCount
    {
        public sealed record Command(int ProductId) : ICommand<Result<bool>>;

        internal sealed class Handler : IRequestHandler<Command, Result<bool>>
        {
            private readonly IGenericRepository<Product, int> _productRepo;

            public Handler(IGenericRepository<Product, int> productRepo)
            {
                _productRepo = productRepo;
            }

            public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
            {
                var product = await _productRepo.GetByIdAsync(request.ProductId, trackChanges: true);

                if (product == null)
                    return Result.Success(false);

                product.ViewCount++;
                product.UpdatedAtUtc = DateTime.UtcNow;

                _productRepo.Update(product);
                await _productRepo.SaveChangesAsync(cancellationToken);

                return Result.Success(true);
            }
        }

    }
}
