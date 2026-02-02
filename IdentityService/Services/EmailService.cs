using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


namespace IdentityService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _environment;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetCode)
        {
            var settings = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings["FromName"], settings["FromAddress"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Password Reset Code";
            message.Body = new BodyBuilder
            {
                HtmlBody = $"""
        <h2>Password Reset</h2>
        <p>Your reset code is:</p>
        <h1>{resetCode}</h1>
        <p>This code will expire in 15 minutes.</p>
        """
            }.ToMessageBody();

            using var client = new SmtpClient();

   
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            try
            {
                await client.ConnectAsync(
                    settings["SmtpServer"],
                    int.Parse(settings["SmtpPort"]!),
                    settings["SmtpPort"] == "465" ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(
                    settings["SmtpUsername"],
                    settings["SmtpPassword"] 
                );

                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
