using Microsoft.Extensions.Options;
using OrderService.Services.DTOs;
using System.Text.Json;

namespace OrderService.Services.Cart
{
    public class CartServiceClient : ICartServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CartServiceClient> _logger;
        private readonly ExternalServicesSettings _settings;

        public CartServiceClient(
            HttpClient httpClient,
            IOptions<ExternalServicesSettings> settings,
            ILogger<CartServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;

            _httpClient.BaseAddress = new Uri(_settings.CartServiceBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutInSeconds);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "OrderService");
        }

        public async Task<CartDto?> GetCartByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/cart/{cartId}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<CartDto>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                _logger.LogWarning("Failed to get cart {CartId}. Status: {StatusCode}",
                    cartId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart {CartId}", cartId);
                return null;
            }
        }

        public async Task<CartDto?> GetActiveCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/cart/user/{userId}/active", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<CartDto>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                _logger.LogWarning("Failed to get active cart for user {UserId}. Status: {StatusCode}",
                    userId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active cart for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/cart/{cartId}", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart {CartId}", cartId);
                return false;
            }
        }

        public async Task<bool> ValidateCartItemsAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"/api/cart/{cartId}/validate",
                    null,
                    cancellationToken);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart {CartId}", cartId);
                return false;
            }
        }
    }
}

