using FluentValidation;
using IdentityService.Data;
using IdentityService.Features.Shared;
using IdentityService.Models;
using IdentityService.Services;
using MediatR;

namespace IdentityService.Features.Commands.PasswordReset
{
    public record RequestPasswordResetCommand(string Email) : IRequest<RequestResponse<RequestPasswordResetResponseDto>>;

    public class RequestPasswordResetResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, RequestResponse<RequestPasswordResetResponseDto>>
    {
        private readonly IRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IdentityDbContext _context;
        private readonly IValidator<RequestPasswordResetCommand> _validator;
        private readonly ILogger<RequestPasswordResetCommandHandler> _logger;

        public RequestPasswordResetCommandHandler(
            IRepository userRepository,
            IEmailService emailService,
            IdentityDbContext context,
            IValidator<RequestPasswordResetCommand> validator,
            ILogger<RequestPasswordResetCommandHandler> logger)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _context = context;
            _validator = validator;
            _logger = logger;
        }

        public async Task<RequestResponse<RequestPasswordResetResponseDto>> Handle(
            RequestPasswordResetCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
             
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return RequestResponse<RequestPasswordResetResponseDto>.Fail(
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        400);
                }

             
                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                   
                    _logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);
                    return RequestResponse<RequestPasswordResetResponseDto>.Success(
                        new RequestPasswordResetResponseDto
                        {
                            Message = "If your email exists, you will receive a reset code",
                            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                        },
                        "Reset code sent",
                        200
                    );
                }

                var resetCode = GenerateResetCode();
                var expiresAt = DateTime.UtcNow.AddMinutes(15);

                var resetRequest = new PasswordResetRequest
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    ResetCode = resetCode,
                    ExpiresAt = expiresAt,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.PasswordResetRequests.Add(resetRequest);
                await _context.SaveChangesAsync();

                await _emailService.SendPasswordResetEmailAsync(request.Email, resetCode);

                _logger.LogInformation("Password reset code generated for {Email}", request.Email);

                return RequestResponse<RequestPasswordResetResponseDto>.Success(
                    new RequestPasswordResetResponseDto
                    {
                        Message = "Reset code sent to your email",
                        ExpiresAt = expiresAt
                    },
                    "Reset code sent",
                    200
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset request for {Email}", request.Email);
                return RequestResponse<RequestPasswordResetResponseDto>.Fail(
                    "An error occurred while processing your request",
                    500);
            }
        }

        private static string GenerateResetCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }

}
