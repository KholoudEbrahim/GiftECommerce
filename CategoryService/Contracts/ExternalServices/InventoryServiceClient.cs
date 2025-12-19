using CategoryService.Contracts.ExternalServices.Dtos;
using System.Text.Json;

namespace CategoryService.Contracts.ExternalServices
{
    public class InventoryServiceClient : IInventoryServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryServiceClient> _logger;

        public InventoryServiceClient(HttpClient httpClient, ILogger<InventoryServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<StockInfoDto?> GetStockInfoAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting stock info for Product {ProductId}", productId);

                var response = await _httpClient.GetAsync(
                    $"/api/inventory/{productId}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("InventoryService returned {StatusCode} for Product {ProductId}",
                        response.StatusCode,
                        productId);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<StockInfoResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null)
                    return null;

                _logger.LogInformation("Stock info retrieved for Product {ProductId}: {CurrentStock}/{MaxStock}",
                    productId,
                    result.CurrentStock,
                    result.MaxStock);

                return new StockInfoDto(
                    result.CurrentStock,
                    result.MinStock,
                    result.MaxStock,
                    result.IsLowStock,
                    result.IsOutOfStock
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock info for ProductId: {ProductId}", productId);
                return null;
            }
        }
    }
    internal record StockInfoResponse(
    int ProductId,
    string ProductName,
    int CurrentStock,
    int MinStock,
    int MaxStock,
    bool IsLowStock,
    bool IsOutOfStock,
    DateTime LastUpdated
    );
}
