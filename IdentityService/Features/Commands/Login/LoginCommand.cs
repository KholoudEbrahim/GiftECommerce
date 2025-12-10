using FluentValidation;
using IdentityService.Data;
using IdentityService.Services;
using IdentityService.Shared;
using MediatR;

namespace IdentityService.Features.Commands.Login
{
    public record LoginCommand(
       string Email,
       string Password
   ) : IRequest<RequestResponse<LoginResponseDto>>
    {
        public class LoginCommandHandler : IRequestHandler<LoginCommand, RequestResponse<LoginResponseDto>>
        {
            private readonly IUserRepository _userRepository;
            private readonly IPasswordService _passwordService;
            private readonly ITokenService _tokenService;
            private readonly IValidator<LoginCommand> _validator;
            private readonly ILogger<LoginCommandHandler> _logger;

            public LoginCommandHandler(
                IUserRepository userRepository,
                IPasswordService passwordService,
                ITokenService tokenService,
                IValidator<LoginCommand> validator,
                ILogger<LoginCommandHandler> logger)
            {
                _userRepository = userRepository;
                _passwordService = passwordService;
                _tokenService = tokenService;
                _validator = validator;
                _logger = logger;
            }

            public async Task<RequestResponse<LoginResponseDto>> Handle(
                LoginCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
           
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        var errorMessages = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                        return RequestResponse<LoginResponseDto>.Fail(errorMessages, 400);
                    }

          
                    var user = await _userRepository.GetByEmailAsync(request.Email);
                    if (user == null)
                    {
                        _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                        return RequestResponse<LoginResponseDto>.Fail("Invalid email or password", 401);
                    }

  
                    if (!user.IsActive)
                    {
                        _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
                        return RequestResponse<LoginResponseDto>.Fail("Account is deactivated", 403);
                    }

                    if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                    {
                        _logger.LogWarning("Invalid password attempt for user: {Email}", request.Email);
                        return RequestResponse<LoginResponseDto>.Fail("Invalid email or password", 401);
                    }


                    user.LastLoginAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);

      
                    var jwtToken = _tokenService.GenerateJwtToken(user);

   
                    var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

      
                    var responseDto = new LoginResponseDto
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role,
                        Token = jwtToken,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                        RefreshToken = refreshToken.Token,
                        RefreshTokenExpiresAt = refreshToken.ExpiresAt
                    };

                    _logger.LogInformation("User logged in successfully: {Email}", user.Email);

                    return RequestResponse<LoginResponseDto>.Success(
                        responseDto,
                        "Login successful",
                        200
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                    return RequestResponse<LoginResponseDto>.Fail("An error occurred during login", 500);
                }
            }
        }
    }
}