using CategoryService.Contracts;
using CategoryService.Contracts.ExternalServices;
using CategoryService.Models;
using CategoryService.shared.MarkerInterface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Products
{
    public static class SyncProductSalesCount
    {
        public sealed record Command(int ProductId) : ICommand<Result<SyncSalesResponse>>;

        internal sealed class Handler : IRequestHandler<Command, Result<SyncSalesResponse>>
        {
            private readonly IGenericRepository<Product, int> _productRepo;
            private readonly IOrderServiceClient _orderServiceClient;
            private readonly ILogger<Handler> _logger;

            public Handler(
                IGenericRepository<Product, int> productRepo,
                IOrderServiceClient orderServiceClient,
                ILogger<Handler> logger)
            {
                _productRepo = productRepo;
                _orderServiceClient = orderServiceClient;
                _logger = logger;
            }


            public async Task<Result<SyncSalesResponse>> Handle(
           Command request,
           CancellationToken cancellationToken)
            {
                _logger.LogInformation(
                    "Syncing sales count for Product {ProductId}",
                    request.ProductId);

                // 1. Get current sales count from Product
                var product = await _productRepo.GetByIdAsync(request.ProductId, trackChanges: true);
                if (product == null)
                    return Result.Failure<SyncSalesResponse>(
                        new Error("Product.NotFound", $"Product {request.ProductId} not found"));

                var oldSalesCount = product.TotalSales;

                // 2. Get actual sales count from OrderService
                var actualSalesCount = await _orderServiceClient.GetProductTotalSalesAsync(
                    request.ProductId,
                    cancellationToken);

                // 3. Update if different
                if (actualSalesCount != oldSalesCount)
                {
                    _logger.LogWarning(
                        "Sales count mismatch for Product {ProductId}: Old={Old}, Actual={Actual}",
                        request.ProductId,
                        oldSalesCount,
                        actualSalesCount);

                    product.TotalSales = actualSalesCount;
                    product.UpdatedAtUtc = DateTime.UtcNow;

                    _productRepo.Update(product);
                    await _productRepo.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Sales count synced for Product {ProductId}: {Old} → {New}",
                        request.ProductId,
                        oldSalesCount,
                        actualSalesCount);
                }
                else
                {
                    _logger.LogInformation(
                        "Sales count already correct for Product {ProductId}: {Count}",
                        request.ProductId,
                        actualSalesCount);
                }

                return Result.Success(new SyncSalesResponse(
                    request.ProductId,
                    oldSalesCount,
                    actualSalesCount,
                    actualSalesCount != oldSalesCount
                ));
            }
        }

    }
    public record SyncSalesResponse(
    int ProductId,
    int OldCount,
    int NewCount,
    bool WasUpdated
    );
    
}
