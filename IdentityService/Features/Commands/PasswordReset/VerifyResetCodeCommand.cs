using FluentValidation;
using IdentityService.Data;
using IdentityService.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Commands.PasswordReset
{
    public record VerifyResetCodeCommand(string Email, string Code)
      : IRequest<RequestResponse<VerifyResetCodeResponseDto>>

    {
        public class VerifyResetCodeCommandHandler
    : IRequestHandler<VerifyResetCodeCommand, RequestResponse<VerifyResetCodeResponseDto>>
        {
            private readonly IdentityDbContext _context;
            private readonly IValidator<VerifyResetCodeCommand> _validator;
            private readonly ILogger<VerifyResetCodeCommandHandler> _logger;

            public VerifyResetCodeCommandHandler(
                IdentityDbContext context,
                IValidator<VerifyResetCodeCommand> validator,
                ILogger<VerifyResetCodeCommandHandler> logger)
            {
                _context = context;
                _validator = validator;
                _logger = logger;
            }

            public async Task<RequestResponse<VerifyResetCodeResponseDto>> Handle(
                VerifyResetCodeCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        return RequestResponse<VerifyResetCodeResponseDto>.Fail(
                            string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                            400);
                    }

         
                    var resetRequest = await _context.PasswordResetRequests
                        .Where(r => r.Email.ToLower() == request.Email.ToLower()
                                 && r.ResetCode == request.Code
                                 && !r.IsUsed
                                 && r.IsActive)
                        .OrderByDescending(r => r.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (resetRequest == null)
                    {
                        return RequestResponse<VerifyResetCodeResponseDto>.Success(
                            new VerifyResetCodeResponseDto
                            {
                                IsValid = false,
                                Message = "Invalid or expired reset code"
                            },
                            "Invalid code",
                            200);
                    }

      
                    if (resetRequest.ExpiresAt < DateTime.UtcNow)
                    {
                        return RequestResponse<VerifyResetCodeResponseDto>.Success(
                            new VerifyResetCodeResponseDto
                            {
                                IsValid = false,
                                Message = "Reset code has expired"
                            },
                            "Code expired",
                            200);
                    }

                    _logger.LogInformation("Reset code verified for {Email}", request.Email);

                    return RequestResponse<VerifyResetCodeResponseDto>.Success(
                        new VerifyResetCodeResponseDto
                        {
                            IsValid = true,
                            Message = "Code verified successfully",
                            ResetRequestId = resetRequest.Id
                        },
                        "Code verified",
                        200);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying reset code for {Email}", request.Email);
                    return RequestResponse<VerifyResetCodeResponseDto>.Fail(
                        "An error occurred while verifying the code",
                        500);
                }
            }
        }



    }
}
