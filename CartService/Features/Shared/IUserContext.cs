namespace CartService.Features.Shared
{
  public interface IUserContext
    {
        Guid? UserId { get; }
        string? AnonymousId { get; }
        bool IsAuthenticated { get; }
    }

    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserContext> _logger;

        public UserContext(
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserContext> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Guid? UserId
        {
            get
            {
                try
                {
                    var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("sub") ??
                                    _httpContextAccessor.HttpContext?.User.FindFirst("userId");

                    if (Guid.TryParse(userIdClaim?.Value, out var userId))
                    {
                        _logger.LogDebug("UserContext: Found UserId: {UserId}", userId);
                        return userId;
                    }

                    _logger.LogDebug("UserContext: No UserId found");
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UserContext: Error getting UserId");
                    return null;
                }
            }
        }

        public string? AnonymousId
        {
            get
            {
                try
                {
                    var httpContext = _httpContextAccessor.HttpContext;
                    if (httpContext == null)
                    {
                        _logger.LogDebug("UserContext: HttpContext is null");
                        return null;
                    }

            
                    var anonymousId = httpContext.Request.Cookies["AnonymousId"];
                    if (string.IsNullOrEmpty(anonymousId))
                    {
                     
                        anonymousId = Guid.NewGuid().ToString();
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = false, 
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTime.UtcNow.AddDays(30)
                        };
                        httpContext.Response.Cookies.Append("AnonymousId", anonymousId, cookieOptions);
                        _logger.LogDebug("UserContext: Generated new AnonymousId: {AnonymousId}", anonymousId);
                    }
                    else
                    {
                        _logger.LogDebug("UserContext: Found existing AnonymousId: {AnonymousId}", anonymousId);
                    }

                    return anonymousId;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UserContext: Error getting/generating AnonymousId");
                    return null;
                }
            }
        }

        public bool IsAuthenticated => UserId.HasValue;
    }

}
