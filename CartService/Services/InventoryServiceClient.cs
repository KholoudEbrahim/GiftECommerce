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
        }

        public async Task<ProductInfoDto> GetProductInfoAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"product:{productId}";

            try
            {
   
                var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
                if (!string.IsNullOrEmpty(cachedData))
                {
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

                return productInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product info for {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> ValidateProductAvailabilityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/products/{productId}/stock?quantity={quantity}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<StockValidationResult>(content, JsonOptions);
                    return result?.IsAvailable ?? false;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating product availability for {ProductId}", productId);
                return false;
            }
        }

        private class StockValidationResult
        {
            public bool IsAvailable { get; set; }
            public int AvailableQuantity { get; set; }
        }

    }

}
