
using CategoryService.Contracts.ExternalServices.Dtos;
using System.Text.Json;

namespace CategoryService.Contracts.ExternalServices
{
    public class OrderServiceClient : IOrderServiceClient
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderServiceClient> _logger;

        public OrderServiceClient(HttpClient httpClient, ILogger<OrderServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<int> GetProductTotalSalesAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting total sales for Product {ProductId}", productId);

                var response = await _httpClient.GetAsync(
                    $"/api/orders/product-sales/{productId}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return 0;

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<ProductSalesResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation("Product {ProductId} total sales: {TotalSales}",
                    productId,
                    result?.TotalQuantitySold ?? 0);

                return result?.TotalQuantitySold ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product sales for ProductId: {ProductId}", productId);
                return 0;
            }
        }
        

        public async Task<bool> IsProductInActiveOrdersAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Checking if Product {ProductId} is in active orders", productId);

                var response = await _httpClient.GetAsync(
                    $"/api/orders/check-product/{productId}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OrderService returned {StatusCode}", response.StatusCode);
                    return true;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<OrderCheckResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation("Product {ProductId} in active orders: {IsInOrders}",
                    productId,
                    result?.IsInActiveOrders ?? false);

                return result?.IsInActiveOrders ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product in orders for ProductId: {ProductId}", productId);
                return true;
            }
        }
    }

    internal record ProductSalesResponse(
    int TotalOrders,
    int TotalQuantitySold,
    decimal TotalRevenue
    );

}
