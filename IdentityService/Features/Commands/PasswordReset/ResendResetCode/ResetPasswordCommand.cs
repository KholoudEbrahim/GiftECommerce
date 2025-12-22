using FluentValidation;
using IdentityService.Data;
using IdentityService.Features.Shared;
using IdentityService.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Commands.PasswordReset.ResendResetCode
{
    public record ResetPasswordCommand(
         Guid ResetRequestId,
         string NewPassword,
         string ConfirmPassword) : IRequest<RequestResponse<ResetPasswordResponseDto>>
    {
        public class ResetPasswordCommandHandler
         : IRequestHandler<ResetPasswordCommand, RequestResponse<ResetPasswordResponseDto>>
        {
            private readonly IRepository _repository;
            private readonly IPasswordService _passwordService;
            private readonly IValidator<ResetPasswordCommand> _validator;
            private readonly ILogger<ResetPasswordCommandHandler> _logger;

            public ResetPasswordCommandHandler(
                IRepository repository,
                IPasswordService passwordService,
                IValidator<ResetPasswordCommand> validator,
                ILogger<ResetPasswordCommandHandler> logger)
            {
                _repository = repository;
                _passwordService = passwordService;
                _validator = validator;
                _logger = logger;
            }

            public async Task<RequestResponse<ResetPasswordResponseDto>> Handle(
                ResetPasswordCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        return RequestResponse<ResetPasswordResponseDto>.Fail(
                            string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                            400);
                    }

                    var resetRequest = await _repository.GetPasswordResetRequestByIdAsync(request.ResetRequestId);

                    if (resetRequest == null || resetRequest.IsUsed || !resetRequest.IsActive)
                    {
                        return RequestResponse<ResetPasswordResponseDto>.Fail(
                            "Invalid or expired reset request",
                            400);
                    }

                    if (resetRequest.ExpiresAt < DateTime.UtcNow)
                    {
                        resetRequest.IsUsed = true;
                        resetRequest.UsedAt = DateTime.UtcNow;
                        await _repository.UpdatePasswordResetRequestAsync(resetRequest);

                        return RequestResponse<ResetPasswordResponseDto>.Fail(
                            "Reset request has expired",
                            400);
                    }

                    var user = await _repository.GetByEmailAsync(resetRequest.Email);
                    if (user == null)
                    {
                        return RequestResponse<ResetPasswordResponseDto>.Fail(
                            "User not found",
                            404);
                    }

                    user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
                    user.UpdatedAt = DateTime.UtcNow;

                    resetRequest.IsUsed = true;
                    resetRequest.UsedAt = DateTime.UtcNow;

                    await _repository.UpdateAsync(user);
                    await _repository.UpdatePasswordResetRequestAsync(resetRequest);

                    _logger.LogInformation("Password reset successful for {Email}", user.Email);

                    return RequestResponse<ResetPasswordResponseDto>.Success(
                        new ResetPasswordResponseDto
                        {
                            Message = "Password has been reset successfully",
                            ResetAt = DateTime.UtcNow
                        },
                        "Password reset successful",
                        200);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resetting password");
                    return RequestResponse<ResetPasswordResponseDto>.Fail(
                        "An error occurred while resetting the password",
                        500);
                }
            }
        }



    }
}
