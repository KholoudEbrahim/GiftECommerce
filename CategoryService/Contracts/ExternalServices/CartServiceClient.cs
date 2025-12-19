
using CategoryService.Contracts.ExternalServices.Dtos;
using System.Text.Json;

namespace CategoryService.Contracts.ExternalServices
{
    public class CartServiceClient : ICartServiceClient
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<CartServiceClient> _logger;

        public CartServiceClient(HttpClient httpClient, ILogger<CartServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<int> GetReservedQuantityAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"/api/cart/reserved-quantity/{productId}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return 0;

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<ReservedQuantityResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Quantity ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reserved quantity for ProductId: {ProductId}", productId);
                return 0;
            }
        }

        public async Task<bool> IsProductInActiveCartsAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Checking if Product {ProductId} is in active carts", productId);


                var response = await _httpClient.GetAsync(
                    $"/api/cart/check-product/{productId}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CartService returned {StatusCode}", response.StatusCode);

                    return true;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<CartCheckResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation("Product {ProductId} in carts: {IsInCart}",
                    productId,
                    result?.IsInCart ?? false);

                return result?.IsInCart ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product in carts for ProductId: {ProductId}", productId);

                return true;
            }
        }

        internal record ReservedQuantityResponse(int Quantity);
    }
}
