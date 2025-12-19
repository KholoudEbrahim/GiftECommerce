using CartService.Services.DTOs;
using CartService.Shared;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CartService.Services
{
    public class ProfileServiceClient : IProfileServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalServicesSettings _settings;
        private readonly ILogger<ProfileServiceClient> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ProfileServiceClient(
            HttpClient httpClient,
            IOptions<ExternalServicesSettings> settings,
            ILogger<ProfileServiceClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_settings.ProfileServiceBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<AddressDto?> GetAddressAsync(Guid addressId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/{userId}/addresses/{addressId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;

                    _logger.LogWarning("Failed to get address {AddressId} for user {UserId}. Status: {StatusCode}",
                        addressId, userId, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<AddressDto>(content, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address {AddressId} for user {UserId}", addressId, userId);
                return null;
            }
        }

        public async Task<List<AddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/{userId}/addresses", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get addresses for user {UserId}. Status: {StatusCode}",
                        userId, response.StatusCode);
                    return new List<AddressDto>();
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<AddressDto>>(content, JsonOptions) ?? new List<AddressDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for user {UserId}", userId);
                return new List<AddressDto>();
            }
        }
    }
}
