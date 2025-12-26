using Microsoft.Extensions.Options;
using OrderService.Services.DTOs;
using System.Text.Json;

namespace OrderService.Services.Inventory
{
    public class InventoryServiceClient : IInventoryServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryServiceClient> _logger;
        private readonly ExternalServicesSettings _settings;

        public InventoryServiceClient(
            HttpClient httpClient,
            IOptions<ExternalServicesSettings> settings,
            ILogger<InventoryServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;

            _httpClient.BaseAddress = new Uri(_settings.InventoryServiceBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutInSeconds);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "OrderService");
        }

        public async Task<ProductDto?> GetProductInfoAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/products/{productId}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                _logger.LogWarning("Failed to get product info {ProductId}. Status: {StatusCode}",
                    productId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product info {ProductId}", productId);
                return null;
            }
        }

        public async Task<bool> ValidateProductAvailabilityAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/products/{productId}/validate",
                    new { Quantity = quantity },
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result?.Data ?? false;
                }

                _logger.LogWarning("Failed to validate product availability {ProductId}. Status: {StatusCode}",
                    productId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating product availability {ProductId}", productId);
                return false;
            }
        }

        public async Task<ProductAvailabilityResponse?> CheckProductAvailabilityAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/products/{productId}/availability?quantity={quantity}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductAvailabilityResponse>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                _logger.LogWarning("Failed to check product availability {ProductId}. Status: {StatusCode}",
                    productId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product availability {ProductId}", productId);
                return null;
            }
        }
    }
}
