namespace IdentityService.Features.Commands.PasswordReset.ResendResetCode
{
    public class ResetPasswordResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public DateTime ResetAt { get; set; }
    }
}
