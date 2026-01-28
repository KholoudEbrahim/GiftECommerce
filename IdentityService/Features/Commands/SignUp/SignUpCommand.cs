using FluentValidation;
using IdentityService.Data;
using IdentityService.Events;
using IdentityService.Features.Shared;
using IdentityService.Models;
using IdentityService.Services;
using MassTransit;
using MediatR;

namespace IdentityService.Features.Commands.SignUp;

public record SignUpCommand(
     string FirstName,
     string LastName,
     string Email,
     string Password,
     string ConfirmPassword,
     string Phone,
     string Gender
 ) : IRequest<RequestResponse<SignUpResponseDto>>
{
    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, RequestResponse<SignUpResponseDto>>
    {
        private readonly IRepository _repository;
        private readonly IPasswordService _passwordService;
        private readonly IValidator<SignUpCommand> _validator;
        private readonly ILogger<SignUpCommandHandler> _logger;
        private readonly IUserEventPublisher _userEventPublisher;

        public SignUpCommandHandler(
            IRepository repository,
            IPasswordService passwordService,
            IValidator<SignUpCommand> validator,
            IUserEventPublisher userEventPublisher,
            ILogger<SignUpCommandHandler> logger)
        {
            _repository = repository;
            _passwordService = passwordService;
            _validator = validator;
            _userEventPublisher = userEventPublisher;
            _logger = logger;
        }

        public async Task<RequestResponse<SignUpResponseDto>> Handle(
       SignUpCommand request,
       CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errorMessages = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    return RequestResponse<SignUpResponseDto>.Fail(errorMessages, 400);
                }

                var normalizedEmail = request.Email.Trim().ToLower();
                var normalizedPhone = request.Phone.Trim();

                if (await _repository.EmailExistsAsync(normalizedEmail))
                    return RequestResponse<SignUpResponseDto>.Fail("Email already registered", 409);

                if (await _repository.PhoneExistsAsync(normalizedPhone))
                    return RequestResponse<SignUpResponseDto>.Fail("Phone number already registered", 409);

                var user = new User
                {
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = normalizedEmail,
                    PasswordHash = _passwordService.HashPassword(request.Password),
                    Phone = normalizedPhone,
                    Gender = request.Gender.Trim().ToLower(),
                    Role = "User",
                    EmailVerified = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _repository.CreateAsync(user);

                try
                {
                    await _userEventPublisher.PublishUserCreatedEventAsync(createdUser, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Event publish failed for {Email}", createdUser.Email);
                }

                return RequestResponse<SignUpResponseDto>.Success(
                    new SignUpResponseDto
                    {
                        UserId = createdUser.Id,
                        Email = createdUser.Email,
                        FirstName = createdUser.FirstName,
                        LastName = createdUser.LastName
                    },
                    "Registration successful",
                    201
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Signup failed for {Email}", request.Email);
                return RequestResponse<SignUpResponseDto>.Fail("An error occurred during registration", 500);
            }
        }

    }
}