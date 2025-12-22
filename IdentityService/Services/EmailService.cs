using System.Net;
using System.Net.Mail;

namespace IdentityService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetCode)
        {
            try
            {
                
                var emailSettings = _configuration.GetSection("EmailSettings");

                using var client = new SmtpClient(emailSettings["SmtpServer"])
                {
                    Port = int.Parse(emailSettings["SmtpPort"]!),
                    Credentials = new NetworkCredential(
                        emailSettings["SmtpUsername"],
                        emailSettings["SmtpPassword"]
                    ),
                    EnableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true")
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        emailSettings["FromAddress"]!,
                        emailSettings["FromName"]
                    ),
                    Subject = "Password Reset Request",
                    Body = $@"
                <h3>Password Reset Request</h3>
                <p>Your password reset code is: <strong>{resetCode}</strong></p>
                <p>This code will expire in 15 minutes.</p>
                <p>If you didn't request this, please ignore this email.</p>
            ",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Password reset email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                throw;
            }
        }
    }
}
