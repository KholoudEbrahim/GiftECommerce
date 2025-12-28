using FluentValidation;
using IdentityService.Data;
using IdentityService.Features.Shared;
using IdentityService.Middlewares;
using IdentityService.Services;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityService.Features.Commands.Login;

public record LoginCommand(
      string Email,
      string Password
  ) : IRequest<RequestResponse<LoginResponseDto>>
{
    public class LoginCommandHandler :
        IRequestHandler<LoginCommand, RequestResponse<LoginResponseDto>>
    {
        private readonly IRepository _repository;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly IValidator<LoginCommand> _validator;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginCommandHandler(
            IRepository repository,
            IPasswordService passwordService,
            ITokenService tokenService,
            IValidator<LoginCommand> validator,
            ILogger<LoginCommandHandler> logger,
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _validator = validator;
            _logger = logger;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
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

                var ipAddress = GetClientIpAddress();

          
                var failedAttempts = FailedLoginTracker.GetFailedAttempts(ipAddress, _cache);
                if (failedAttempts >= 10)
                {
                    _logger.LogWarning("Blocked login attempt from IP {IpAddress} - too many failed attempts", ipAddress);
                    return RequestResponse<LoginResponseDto>.Fail(
                        "Too many failed attempts. Please try again after 15 minutes.",
                        429);
                }

                var user = await _repository.GetByEmailAsync(request.Email);

                if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    
                    FailedLoginTracker.RecordFailedAttempt(ipAddress, _cache);

                    _logger.LogWarning("Failed login attempt for email: {Email} from IP: {IpAddress}",
                        request.Email, ipAddress);
                    return RequestResponse<LoginResponseDto>.Fail("Invalid email or password", 401);
                }

                FailedLoginTracker.ClearFailedAttempts(ipAddress, _cache);

                
                user.LastLoginAt = DateTime.UtcNow;
                await _repository.UpdateAsync(user);

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
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
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

        private string GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return string.Empty;

            
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                return forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            }

            if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
            {
                return realIp.FirstOrDefault();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }
}
