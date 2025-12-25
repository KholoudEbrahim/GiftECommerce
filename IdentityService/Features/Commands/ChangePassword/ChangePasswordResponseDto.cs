namespace IdentityService.Features.Commands.ChangePassword
{
    public record ChangePasswordResponseDto
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public DateTime UpdatedAt { get; init; }
    }
}
