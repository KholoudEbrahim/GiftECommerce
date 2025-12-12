namespace IdentityService.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetCode);
    }
}
