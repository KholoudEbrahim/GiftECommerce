using CartService.Services.DTOs;
using CartService.Shared;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CartService.Services
{
    public class InventoryServiceClient : IInventoryServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _cache;
        private readonly ExternalServicesSettings _settings;
        private readonly ILogger<InventoryServiceClient> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public InventoryServiceClient(
            HttpClient httpClient,
            IDistributedCache cache,
            IOptions<ExternalServicesSettings> settings,
            ILogger<InventoryServiceClient> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _settings = settings.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_settings.InventoryServiceBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.Timeout = TimeSpan.FromSeconds(10); 
        }

        public async Task<ProductInfoDto> GetProductInfoAsync(int productId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product info for ProductId: {ProductId}", productId);


            return productId switch
            {
                1 => new ProductInfoDto
                {
                    Id = 1,
                    Name = "Red Rose Bouquet",
                    Price = 299.99m,
                    Discount = 249.99m,
                    ImageUrl = "https://localhost:7139/images/products/red-roses.jpg",
                    IsActive = true,
                    Status = "InStock"
                },
                2 => new ProductInfoDto
                {
                    Id = 2,
                    Name = "White Lily Arrangement",
                    Price = 399.99m,
                    Discount = null,
                    ImageUrl = "https://localhost:7139/images/products/white-lilies.jpg",
                    IsActive = true,
                    Status = "InStock"
                },
                _ => new ProductInfoDto
                {
                    Id = productId,
                    Name = $"Product {productId}",
                    Price = 100.00m,
                    ImageUrl = "https://example.com/product.jpg",
                    IsActive = true,
                    Status = "InStock"
                }
            };
        }

        public async Task<bool> ValidateProductAvailabilityAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Validating availability for ProductId: {ProductId}, Quantity: {Quantity}",
                productId, quantity);

      
            return true;
        }

        private async Task<ProductInfoDto> GetProductInfoFromServiceAsync(int productId, CancellationToken cancellationToken)
        {
            var cacheKey = $"product:{productId}";

            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogDebug("Product {ProductId} found in cache", productId);
                    return JsonSerializer.Deserialize<ProductInfoDto>(cachedData, JsonOptions)!;
                }

                var response = await _httpClient.GetAsync($"/api/products/{productId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get product info for {ProductId}. Status: {StatusCode}",
                        productId, response.StatusCode);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        throw new KeyNotFoundException($"Product {productId} not found in inventory");

                    throw new HttpRequestException($"Failed to get product info. Status: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var productInfo = JsonSerializer.Deserialize<ProductInfoDto>(content, JsonOptions);

                if (productInfo == null)
                    throw new InvalidOperationException("Failed to deserialize product info");

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                await _cache.SetStringAsync(cacheKey, content, cacheOptions, cancellationToken);
                _logger.LogDebug("Product {ProductId} cached successfully", productId);

                return productInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product info for {ProductId}", productId);
                throw;
            }
        }
        private async Task<bool> ValidateAvailabilityFromServiceAsync(int productId, int quantity, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/inventory/check/{productId}?quantity={quantity}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<StockCheckResponse>(content, JsonOptions);
                    return result?.IsAvailable ?? false;
                }

                _logger.LogWarning("Inventory service returned {StatusCode} for product {ProductId}",
                    response.StatusCode, productId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating product availability for {ProductId}", productId);
                return false;
            }
        }

        private class StockCheckResponse
        {
            public bool IsAvailable { get; set; }
            public int AvailableQuantity { get; set; }
        }
    }
}
