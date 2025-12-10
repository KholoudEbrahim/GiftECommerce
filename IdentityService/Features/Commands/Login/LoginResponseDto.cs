namespace IdentityService.Features.Commands.Login
{
    public record LoginResponseDto
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public string Token { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
        public string RefreshToken { get; init; } = string.Empty;
        public DateTime RefreshTokenExpiresAt { get; init; }
    }
}
