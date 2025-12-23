using IdentityService.Models;

namespace IdentityService.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
        string GenerateRefreshToken();
        Task<RefreshToken> CreateRefreshTokenAsync(Guid userId);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string? replacedByToken = null);
        Task RevokeDescendantRefreshTokensAsync(RefreshToken refreshToken, string newToken);
    }
}
