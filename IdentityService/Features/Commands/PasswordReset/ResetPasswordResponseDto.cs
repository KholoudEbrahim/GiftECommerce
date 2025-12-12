namespace IdentityService.Features.Commands.PasswordReset
{
    public class ResetPasswordResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public DateTime ResetAt { get; set; }
    }
}
