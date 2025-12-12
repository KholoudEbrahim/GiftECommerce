using FluentValidation;
using IdentityService.Data;
using IdentityService.Features.Shared;
using IdentityService.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Commands.PasswordReset
{
    public record ResetPasswordCommand(
         Guid ResetRequestId,
         string NewPassword,
         string ConfirmPassword) : IRequest<RequestResponse<ResetPasswordResponseDto>>
    {
        public class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, RequestResponse<ResetPasswordResponseDto>>
        {
            private readonly IRepository _userRepository;
            private readonly IPasswordService _passwordService;
            private readonly IdentityDbContext _context;
            private readonly IValidator<ResetPasswordCommand> _validator;
            private readonly ILogger<ResetPasswordCommandHandler> _logger;

            public ResetPasswordCommandHandler(
                IRepository userRepository,
                IPasswordService passwordService,
                IdentityDbContext context,
                IValidator<ResetPasswordCommand> validator,
                ILogger<ResetPasswordCommandHandler> logger)
            {
                _userRepository = userRepository;
                _passwordService = passwordService;
                _context = context;
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

                    var resetRequest = await _context.PasswordResetRequests
    .FirstOrDefaultAsync(r => r.Id == request.ResetRequestId
                           && !r.IsUsed
                           && r.IsActive, cancellationToken);

                    if (resetRequest == null)
                    {
                        return RequestResponse<ResetPasswordResponseDto>.Fail(
                            "Invalid or expired reset request",
                            400);
                    }

   
                    if (resetRequest.ExpiresAt < DateTime.UtcNow)
                    {
                        resetRequest.IsUsed = true;
                        resetRequest.UsedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync(cancellationToken);

                        return RequestResponse<ResetPasswordResponseDto>.Fail(
                            "Reset request has expired",
                            400);
                    }

             
                    var user = await _userRepository.GetByEmailAsync(resetRequest.Email);
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

                    await _context.SaveChangesAsync(cancellationToken);

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
