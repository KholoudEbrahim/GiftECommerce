using FluentValidation;
using IdentityService.Data;
using IdentityService.Features.Shared;
using IdentityService.Services;
using MediatR;

namespace IdentityService.Features.Commands.Login;

public record LoginCommand(string Email,string Password) : IRequest<RequestResponse<LoginResponseDto>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, RequestResponse<LoginResponseDto>>
{
<<<<<<< HEAD
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
=======
    public record LoginCommand(
       string Email,
       string Password,
       string? RequiredRole = null 
   ) : IRequest<RequestResponse<LoginResponseDto>>
    {
        public class LoginCommandHandler : IRequestHandler<LoginCommand, RequestResponse<LoginResponseDto>>
        {
            private readonly IRepository _userRepository;
            private readonly IPasswordService _passwordService;
            private readonly ITokenService _tokenService;
            private readonly IValidator<LoginCommand> _validator;
            private readonly ILogger<LoginCommandHandler> _logger;

            public LoginCommandHandler(
                IRepository userRepository,
                IPasswordService passwordService,
                ITokenService tokenService,
                IValidator<LoginCommand> validator,
                ILogger<LoginCommandHandler> logger)
>>>>>>> 618ef798b400f9239616a518e96b7c277770fe3c
            {
       
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
<<<<<<< HEAD
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
=======
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


                    if (!string.IsNullOrEmpty(request.RequiredRole)
                        && !string.Equals(user.Role, request.RequiredRole, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning(
                            "Role mismatch for {Email}. Required: {RequiredRole}, Actual: {ActualRole}",
                            request.Email, request.RequiredRole, user.Role);
                        return RequestResponse<LoginResponseDto>.Fail("Access denied. Insufficient permissions.", 403);
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

                    _logger.LogInformation("User logged in successfully: {Email} (Role: {Role})",
                        user.Email, user.Role);

                    return RequestResponse<LoginResponseDto>.Success(
                        responseDto,
                        "Login successful",
                        200
                    );
                }
                catch (Exception ex)
>>>>>>> 618ef798b400f9239616a518e96b7c277770fe3c
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
