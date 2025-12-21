using InventoryService.Contracts;
using InventoryService.Models;
using InventoryService.shared.MarkerInterface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace InventoryService.Features
{
    public static class BulkCheckStockAvailability
    {
        public sealed record Query(
       List<ProductQuantityRequest> Products
       ):ICommand<Result<BulkCheckStockResponse>>;

        public record ProductQuantityRequest(int ProductId, int Quantity);

        internal sealed class Handler : IRequestHandler<Query, Result<BulkCheckStockResponse>>
        {
            private readonly IGenericRepository<Stock, int> _stockRepo;
            private readonly ILogger<Handler> _logger;

            public Handler(
                IGenericRepository<Stock, int> stockRepo,
                ILogger<Handler> logger)
            {
                _stockRepo = stockRepo;
                _logger = logger;
            }

            public async Task<Result<BulkCheckStockResponse>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                _logger.LogInformation(
                    "Bulk checking stock for {Count} products",
                    request.Products.Count);

                var productIds = request.Products.Select(p => p.ProductId).ToList();

                // Get all stocks in one query
                var stocks = await _stockRepo
                    .GetAll(s => productIds.Contains(s.ProductId), trackChanges: false)
                    .ToDictionaryAsync(s => s.ProductId, cancellationToken);

                var results = new List<ProductStockCheckResult>();
                bool allAvailable = true;

                foreach (var product in request.Products)
                {
                    if (!stocks.TryGetValue(product.ProductId, out var stock))
                    {
                        // Stock record not found
                        results.Add(new ProductStockCheckResult(
                            product.ProductId,
                            false,
                            0,
                            product.Quantity,
                            "Stock record not found"
                        ));
                        allAvailable = false;
                        continue;
                    }

                    var isAvailable = stock.CurrentStock >= product.Quantity;
                    if (!isAvailable)
                        allAvailable = false;

                    var message = isAvailable
                        ? "Available"
                        : stock.CurrentStock == 0
                            ? "Out of stock"
                            : $"Insufficient (Available: {stock.CurrentStock})";

                    results.Add(new ProductStockCheckResult(
                        product.ProductId,
                        isAvailable,
                        stock.CurrentStock,
                        product.Quantity,
                        message
                    ));
                }

                _logger.LogInformation(
                    "Bulk check completed: {Available}/{Total} products available",
                    results.Count(r => r.IsAvailable),
                    results.Count);

                return Result.Success(new BulkCheckStockResponse(
                    allAvailable,
                    results
                ));
            }
        }



    }

    public record BulkCheckStockResponse(
    bool AllAvailable,
    List<ProductStockCheckResult> Results
    );

    public record ProductStockCheckResult(
        int ProductId,
        bool IsAvailable,
        int CurrentStock,
        int RequestedQuantity,
        string Message
    );

}
