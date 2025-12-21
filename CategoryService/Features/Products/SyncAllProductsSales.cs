using CategoryService.Contracts;
using CategoryService.Models;
using CategoryService.shared.MarkerInterface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Products
{
    public static class SyncAllProductsSales
    {
        public sealed record Command : ICommand<Result<SyncAllSalesResponse>>;


        internal sealed class Handler : IRequestHandler<Command, Result<SyncAllSalesResponse>>
        {
            private readonly IGenericRepository<Product, int> _productRepo;
            private readonly ISender _sender;
            private readonly ILogger<Handler> _logger;

            public Handler(
                IGenericRepository<Product, int> productRepo,
                ISender sender,
                ILogger<Handler> logger)
            {
                _productRepo = productRepo;
                _sender = sender;
                _logger = logger;
            }

            public async Task<Result<SyncAllSalesResponse>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                _logger.LogInformation("Starting sync for all products sales...");

                var products = await _productRepo
                    .GetAll(trackChanges: false)
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);

                int totalProducts = products.Count;
                int updatedCount = 0;
                var errors = new List<string>();

                foreach (var productId in products)
                {
                    try
                    {
                        var syncCommand = new SyncProductSalesCount.Command(productId);
                        var result = await _sender.Send(syncCommand, cancellationToken);

                        if (result.IsSuccess && result.Value.WasUpdated)
                        {
                            updatedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing Product {ProductId}", productId);
                        errors.Add($"Product {productId}: {ex.Message}");
                    }
                }

                _logger.LogInformation(
                    "Sync completed: {Updated}/{Total} products updated",
                    updatedCount,
                    totalProducts);

                return Result.Success(new SyncAllSalesResponse(
                    totalProducts,
                    updatedCount,
                    errors
                ));
            }
        }


    }


    public record SyncAllSalesResponse(
    int TotalProducts,
    int UpdatedCount,
    List<string> Errors
    );
}
