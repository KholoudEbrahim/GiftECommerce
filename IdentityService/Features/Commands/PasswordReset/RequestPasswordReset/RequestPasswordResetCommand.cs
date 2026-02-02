using FluentValidation;
using IdentityService.Data;
using IdentityService.Features.Shared;
using IdentityService.Models;
using IdentityService.Services;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace IdentityService.Features.Commands.PasswordReset.RequestPasswordReset
{
    public record RequestPasswordResetCommand(string Email) : IRequest<RequestResponse<RequestPasswordResetResponseDto>>;

    public class RequestPasswordResetResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class RequestPasswordResetCommandHandler
        : IRequestHandler<RequestPasswordResetCommand, RequestResponse<RequestPasswordResetResponseDto>>
    {
        private readonly IRepository _repository;
        private readonly IEmailService _emailService;
        private readonly IValidator<RequestPasswordResetCommand> _validator;
        private readonly ILogger<RequestPasswordResetCommandHandler> _logger;
        private readonly IConfiguration _configuration;
        public RequestPasswordResetCommandHandler(
            IRepository repository,
            IEmailService emailService,
            IValidator<RequestPasswordResetCommand> validator,
            ILogger<RequestPasswordResetCommandHandler> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _emailService = emailService;
            _validator = validator;
            _logger = logger;
            _configuration = configuration;
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

                var user = await _repository.GetByEmailAsync(request.Email);
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

                var resetCode = ResetCodeGenerator.Generate(
                 _configuration.GetValue<int>("PasswordReset:CodeLength")
                                 );

                var expiresAt = DateTime.UtcNow.AddMinutes(
                                _configuration.GetValue<int>("PasswordReset:CodeExpirationMinutes")
                            );

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

                await _repository.CreatePasswordResetRequestAsync(resetRequest);

                try
                {
                    await _emailService.SendPasswordResetEmailAsync(request.Email, resetCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Email sending failed for {Email}. Reset code is still valid.",
                        request.Email);
                }


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


    }

}
