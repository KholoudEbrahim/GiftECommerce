using FluentValidation;
using IdentityService.Data;
using IdentityService.Events;
using IdentityService.Features.Shared;
using IdentityService.Models;
using IdentityService.Services;
using MassTransit;
using MediatR;

namespace IdentityService.Features.Commands.ChangePassword
{
    public record ChangePasswordCommand(
       Guid UserId,                
       string CurrentPassword,       
       string NewPassword,            
       string ConfirmNewPassword     
   ) : IRequest<RequestResponse<ChangePasswordResponseDto>>
    {
        public class ChangePasswordCommandHandler :
     IRequestHandler<ChangePasswordCommand, RequestResponse<ChangePasswordResponseDto>>
        {
            private readonly IRepository _repository;
            private readonly IPasswordService _passwordService;
            private readonly IValidator<ChangePasswordCommand> _validator;
            private readonly ILogger<ChangePasswordCommandHandler> _logger;
            private readonly IPublishEndpoint _publishEndpoint;

            public ChangePasswordCommandHandler(
                IRepository repository,
                IPasswordService passwordService,
                IValidator<ChangePasswordCommand> validator,
                ILogger<ChangePasswordCommandHandler> logger,
                IPublishEndpoint publishEndpoint)
            {
                _repository = repository;
                _passwordService = passwordService;
                _validator = validator;
                _logger = logger;
                _publishEndpoint = publishEndpoint;
            }

            public async Task<RequestResponse<ChangePasswordResponseDto>> Handle(
                ChangePasswordCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                  
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        var errorMessages = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                        return RequestResponse<ChangePasswordResponseDto>.Fail(errorMessages, 400);
                    }

                 
                    var user = await _repository.GetByIdAsync(request.UserId);
                    if (user == null)
                    {
                        _logger.LogWarning("Change password attempt for non-existent user: {UserId}", request.UserId);
                        return RequestResponse<ChangePasswordResponseDto>.Fail("User not found", 404);
                    }

             
                    if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                    {
                        _logger.LogWarning("Invalid current password for user: {Email}", user.Email);
                        return RequestResponse<ChangePasswordResponseDto>.Fail("Current password is incorrect", 400);
                    }

                
                    if (_passwordService.VerifyPassword(request.NewPassword, user.PasswordHash))
                    {
                        _logger.LogWarning("New password is same as old password for user: {Email}", user.Email);
                        return RequestResponse<ChangePasswordResponseDto>.Fail("New password must be different from current password", 400);
                    }

                   
                    user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
                    user.UpdatedAt = DateTime.UtcNow;

                    await _repository.UpdateAsync(user);

                    await _repository.RevokeAllRefreshTokensForUserAsync(user.Id);

                   
                    await PublishPasswordChangedEvent(user);

                  
                    var responseDto = new ChangePasswordResponseDto
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        UpdatedAt = user.UpdatedAt.Value
                    };

                    _logger.LogInformation("Password changed successfully for user: {Email}", user.Email);

                    return RequestResponse<ChangePasswordResponseDto>.Success(
                        responseDto,
                        "Password changed successfully",
                        200
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error changing password for user ID: {UserId}", request.UserId);
                    return RequestResponse<ChangePasswordResponseDto>.Fail("An error occurred while changing password", 500);
                }
            }

            private async Task PublishPasswordChangedEvent(User user)
            {
                try
                {
                    var @event = new PasswordChangedEvent
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        ChangedAt = DateTime.UtcNow
                    };

                    await _publishEndpoint.Publish(@event);

                    _logger.LogInformation(
                        "PasswordChangedEvent published for user: {Email}",
                        user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish PasswordChangedEvent for user: {Email}", user.Email);
                  
                }
            }
        }
    }
}
