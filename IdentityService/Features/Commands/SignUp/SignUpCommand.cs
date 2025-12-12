using FluentValidation;
using IdentityService.Data;
using IdentityService.Features.Shared;
using IdentityService.Models;
using IdentityService.Services;
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
        private readonly IRepository _Repository;
        private readonly IPasswordService _passwordService;
        private readonly IValidator<SignUpCommand> _validator;
        private readonly ILogger<SignUpCommandHandler> _logger;

        public SignUpCommandHandler(
            IRepository Repository,
            IPasswordService passwordService,
            IValidator<SignUpCommand> validator,
            ILogger<SignUpCommandHandler> logger)
        {
            _Repository = Repository;
            _passwordService = passwordService;
            _validator = validator;
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

                if (await _Repository.EmailExistsAsync(request.Email))
                {
                    return RequestResponse<SignUpResponseDto>.Fail("Email already registered", 409);
                }

                if (await _Repository.PhoneExistsAsync(request.Phone))
                {
                    return RequestResponse<SignUpResponseDto>.Fail("Phone number already registered", 409);
                }

                var user = new User
                {
                 
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = request.Email.ToLower().Trim(),
                    PasswordHash = _passwordService.HashPassword(request.Password),
                    Phone = request.Phone.Trim(),
                    Gender = request.Gender.Trim(),
                    Role = "User",                
                    EmailVerified = false
                };

                var createdUser = await _Repository.CreateAsync(user);

                var responseDto = new SignUpResponseDto
                {
                    UserId = createdUser.Id, 
                    Email = createdUser.Email,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName
                };

                _logger.LogInformation("User registered successfully: {Email}", createdUser.Email);

                return RequestResponse<SignUpResponseDto>.Success(
                    responseDto,
                    "Registration successful. Please login.",
                    201
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
                return RequestResponse<SignUpResponseDto>.Fail("An error occurred during registration", 500);
            }
        }
    }
}

