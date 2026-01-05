using System.Security.Claims;

namespace UserProfileService.Infrastructure
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                throw new UnauthorizedAccessException("UserId claim is missing");

            return Guid.Parse(userIdClaim);
        }
    }
}
