namespace IdentityService.Features.Commands.PasswordReset.VerifyResetCode
{
    public class VerifyResetCodeResponseDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid ResetRequestId { get; set; }
    }
}
