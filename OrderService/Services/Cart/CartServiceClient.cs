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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartServiceClient(
            HttpClient httpClient,
            IOptions<ExternalServicesSettings> settings,
            ILogger<CartServiceClient> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;
            _httpContextAccessor = httpContextAccessor;

            _httpClient.BaseAddress = new Uri(_settings.CartServiceBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutInSeconds);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "OrderService");
        }

        public async Task<CartDto?> GetActiveCartByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/cart?includeAddressDetails=true");

                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Failed to get active cart for user {UserId}. Status: {StatusCode}",
                        userId,
                        response.StatusCode);

                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<CartDto>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active cart for user {UserId}", userId);
                return null;
            }
        }


        public async Task<bool> ValidateActiveCartAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var cart = await GetActiveCartByUserIdAsync(userId, cancellationToken);

            return cart != null &&
                   cart.Items != null &&
                   cart.Items.Any();
        }

        public async Task<bool> ClearCartAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, "/api/cart");
                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Active cart cleared successfully");
                    return true;
                }

                _logger.LogWarning(
                    "Failed to clear active cart. Status: {StatusCode}",
                    response.StatusCode);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing active cart");
                return false;
            }
        }


        private void AddAuthorizationHeader(HttpRequestMessage request)
        {
            var token = GetUserToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }
        }

        private string? GetUserToken()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var authHeader = httpContext?.Request?.Headers?.Authorization.FirstOrDefault();

                if (authHeader != null &&
                    authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return authHeader["Bearer ".Length..].Trim();
                }

                _logger.LogWarning("Authorization header not found");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting user token");
                return null;
            }
        }
    }
}
