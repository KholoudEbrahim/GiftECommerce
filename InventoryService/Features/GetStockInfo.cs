using InventoryService.Contracts;
using InventoryService.Contracts.Stock;
using InventoryService.Models;
using InventoryService.shared.MarkerInterface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace InventoryService.Features
{
    public static class GetStockInfo
    {
        public sealed record Query(int ProductId) : ICommand<Result<GetStockInfoResponse>>;

        internal sealed class Handler : IRequestHandler<Query, Result<GetStockInfoResponse>>
        {
            private readonly IGenericRepository<Stock, int> _stockRepo;
            private readonly HttpClient _httpClient;
            private readonly ILogger<Handler> _logger;

            public Handler(IGenericRepository<Stock, int> stockRepo, HttpClient httpClient, ILogger<Handler> logger)
            {
                _stockRepo = stockRepo;
                _httpClient = httpClient;
                _logger = logger;
            }

            public async Task<Result<GetStockInfoResponse>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var stock = await _stockRepo
                    .GetAll(s => s.ProductId == request.ProductId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (stock == null)
                    return Result.Failure<GetStockInfoResponse>(
                        new Error("Stock.NotFound",
                            $"Stock information for product {request.ProductId} not found"));

                // Get reserved quantity from CartService
                int reservedQuantity = 0;
                try
                {
                    var newresponse = await _httpClient.GetAsync(
                        $"/api/cart/reserved-quantity/{request.ProductId}",
                        cancellationToken);

                    if (newresponse.IsSuccessStatusCode)
                    {
                        var content = await newresponse.Content.ReadAsStringAsync(cancellationToken);
                        var reservedData = System.Text.Json.JsonSerializer.Deserialize<ReservedQuantityDto>(
                            content,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        reservedQuantity = reservedData?.Quantity ?? 0;

                        _logger.LogInformation(
                            "Reserved quantity for Product {ProductId}: {Reserved}",
                            request.ProductId,
                            reservedQuantity);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Could not fetch reserved quantity for Product {ProductId}, assuming 0",
                        request.ProductId);
                }

                // Calculate available stock
                var availableStock = stock.CurrentStock - reservedQuantity;


                var response = new GetStockInfoResponse(
                    stock.ProductId,
                    stock.ProductName,
                    stock.CurrentStock,
                    stock.MinStock,
                    stock.MaxStock,
                    stock.IsLowStock,
                    stock.IsOutOfStock,
                    stock.UpdatedAtUtc ?? stock.CreatedAtUtc
                );

                return Result.Success(response);
            }
        }

        private record ReservedQuantityDto(int ProductId, int Quantity);

    }
}
