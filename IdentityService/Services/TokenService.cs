using IdentityService.Data;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IdentityDbContext _context;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            IConfiguration configuration,
            IdentityDbContext context,
            ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT Secret not configured"));

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new(ClaimTypes.Role, user.Role),
                new("email_verified", user.EmailVerified.ToString().ToLower())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(
                    _configuration["Jwt:TokenExpirationMinutes"] ?? "15")),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToDouble(
                    _configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);
        }

        public async Task RevokeRefreshTokenAsync(string token, string? replacedByToken = null)
        {
            var refreshToken = await GetRefreshTokenAsync(token);
            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                if (!string.IsNullOrEmpty(replacedByToken))
                {
                    refreshToken.ReplacedByToken = replacedByToken;
                }
                refreshToken.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeDescendantRefreshTokensAsync(RefreshToken refreshToken, string newToken)
        {

            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = await GetRefreshTokenAsync(refreshToken.ReplacedByToken);
                if (childToken != null && childToken.IsActive)
                {
                    childToken.IsRevoked = true;
                    childToken.ReplacedByToken = newToken;
                    childToken.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await RevokeDescendantRefreshTokensAsync(childToken, newToken);
                }
            }
        }
    }
}