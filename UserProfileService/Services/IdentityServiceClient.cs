using Polly;
using Polly.Retry;

namespace UserProfileService.Services
{
    public class IdentityServiceClient : IIdentityServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IdentityServiceClient> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public IdentityServiceClient(HttpClient httpClient, ILogger<IdentityServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(x => (int)x.StatusCode >= 500)
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogWarning(
                            "Delaying for {delay}ms, then making retry {retryAttempt}.",
                            timespan.TotalMilliseconds, retryAttempt);
                    });
        }

        public async Task<UserIdentityDto?> GetUserIdentityAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.GetAsync($"api/users/{userId}", cancellationToken));

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Identity service returned {StatusCode} for user {UserId}",
                        response.StatusCode, userId);

                    return null;
                }


                return await response.Content.ReadFromJsonAsync<UserIdentityDto>(cancellationToken: cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Identity Service for user {UserId}", userId);
                return null;
            }
        }
    }

    public class UserIdentityDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
       
    }
}
