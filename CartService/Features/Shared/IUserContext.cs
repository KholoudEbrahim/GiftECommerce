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

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("sub") ??
                                _httpContextAccessor.HttpContext?.User.FindFirst("userId");

                if (Guid.TryParse(userIdClaim?.Value, out var userId))
                    return userId;

                return null;
            }
        }

        public string? AnonymousId
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.Request.Headers.TryGetValue("X-Anonymous-Id", out var anonymousId) == true)
                    return anonymousId;

                return null;
            }
        }

        public bool IsAuthenticated => UserId.HasValue;
    }
}
