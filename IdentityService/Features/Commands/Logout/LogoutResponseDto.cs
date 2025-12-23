namespace IdentityService.Features.Commands.Logout
{
    public record LogoutResponseDto
    {
        public string Message { get; init; } = string.Empty;  
        public DateTime LoggedOutAt { get; init; } = DateTime.UtcNow;
    }

}
