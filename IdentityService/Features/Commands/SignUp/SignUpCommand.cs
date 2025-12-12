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
) : IRequest<RequestResponse<SignUpResponseDto>>;

public class SignUpCommandHandler : IRequestHandler<SignUpCommand, RequestResponse<SignUpResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IValidator<SignUpCommand> _validator;
    private readonly ILogger<SignUpCommandHandler> _logger;

    public SignUpCommandHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IValidator<SignUpCommand> validator,
        ILogger<SignUpCommandHandler> logger)
    {
<<<<<<< HEAD
        _userRepository = userRepository;
        _passwordService = passwordService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<RequestResponse<SignUpResponseDto>> Handle(
        SignUpCommand request,
        CancellationToken cancellationToken)
    {
        try
=======
        private readonly IRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IValidator<SignUpCommand> _validator;
        private readonly ILogger<SignUpCommandHandler> _logger;

        public SignUpCommandHandler(
            IRepository userRepository,
            IPasswordService passwordService,
            IValidator<SignUpCommand> validator,
            ILogger<SignUpCommandHandler> logger)
>>>>>>> 618ef798b400f9239616a518e96b7c277770fe3c
        {

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return RequestResponse<SignUpResponseDto>.Fail(errorMessages, 400);
            }


            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                return RequestResponse<SignUpResponseDto>.Fail("Email already registered", 409);
            }

            if (await _userRepository.PhoneExistsAsync(request.Phone))
            {
                return RequestResponse<SignUpResponseDto>.Fail("Phone number already registered", 409);
            }

  
            var user = new User
            {
                Id = Guid.NewGuid(), 
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.ToLower().Trim(),
                PasswordHash = _passwordService.HashPassword(request.Password),
                Phone = request.Phone.Trim(),
                Gender = request.Gender.Trim(),
                Role = "User", 
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailVerified = false
            };


            var createdUser = await _userRepository.CreateAsync(user);

            var responseDto = new SignUpResponseDto
            {
                UserId = createdUser.Id,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
  
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
