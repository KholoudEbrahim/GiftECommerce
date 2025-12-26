using Microsoft.Extensions.Options;
using OrderService.Services.DTOs;
using System.Text.Json;

namespace OrderService.Services.UserProfile
{
    public class ProfileServiceClient : IProfileServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProfileServiceClient> _logger;
        private readonly ExternalServicesSettings _settings;

        public ProfileServiceClient(
            HttpClient httpClient,
            IOptions<ExternalServicesSettings> settings,
            ILogger<ProfileServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;

            _httpClient.BaseAddress = new Uri(_settings.ProfileServiceBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutInSeconds);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "OrderService");
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/{userId}/profile", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserProfileDto>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                _logger.LogWarning("Failed to get user profile {UserId}. Status: {StatusCode}",
                    userId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile {UserId}", userId);
                return null;
            }
        }

        public async Task<List<AddressDto>?> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/{userId}/addresses", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<AddressDto>>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                _logger.LogWarning("Failed to get user addresses {UserId}. Status: {StatusCode}",
                    userId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user addresses {UserId}", userId);
                return null;
            }
        }

        public async Task<AddressDto?> GetAddressByIdAsync(Guid addressId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/addresses/{addressId}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AddressDto>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                _logger.LogWarning("Failed to get address {AddressId}. Status: {StatusCode}",
                    addressId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address {AddressId}", addressId);
                return null;
            }
        }
    }
}
