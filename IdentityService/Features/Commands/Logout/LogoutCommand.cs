using FluentValidation;
using IdentityService.Data;
using IdentityService.Features.Shared;
using IdentityService.Services;
using MediatR;

namespace IdentityService.Features.Commands.Logout
{
    public record LogoutCommand(
       string RefreshToken
   ) : IRequest<RequestResponse<LogoutResponseDto>>;

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, RequestResponse<LogoutResponseDto>>
    {
        private readonly IRepository _repository;
        private readonly ITokenService _tokenService;
        private readonly IValidator<LogoutCommand> _validator;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(
            IRepository repository,
            ITokenService tokenService,
            IValidator<LogoutCommand> validator,
            ILogger<LogoutCommandHandler> logger)
        {
            _repository = repository;
            _tokenService = tokenService;
            _validator = validator;
            _logger = logger;
        }

        public async Task<RequestResponse<LogoutResponseDto>> Handle(
            LogoutCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
          
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errorMessages = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    return RequestResponse<LogoutResponseDto>.Fail(errorMessages, 400);
                }

              
                var refreshToken = await _tokenService.GetRefreshTokenAsync(request.RefreshToken);

                if (refreshToken == null)
                {
                   
                    _logger.LogInformation("Logout attempted with non-existent refresh token");
                    return RequestResponse<LogoutResponseDto>.Success(
                        new LogoutResponseDto { Message = "Logged out successfully" },
                        "Logged out successfully",
                        200
                    );
                }

              
                if (refreshToken.IsRevoked)
                {
                    return RequestResponse<LogoutResponseDto>.Success(
                        new LogoutResponseDto { Message = "Already logged out" },
                        "Already logged out",
                        200
                    );
                }

                await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

      
                await _tokenService.RevokeDescendantRefreshTokensAsync(refreshToken, string.Empty);

                _logger.LogInformation("User logged out successfully. UserId: {UserId}, TokenId: {TokenId}",
                    refreshToken.UserId, refreshToken.Id);

                return RequestResponse<LogoutResponseDto>.Success(
                    new LogoutResponseDto { Message = "Logged out successfully" },
                    "Logged out successfully",
                    200
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for refresh token");
                return RequestResponse<LogoutResponseDto>.Fail("An error occurred during logout", 500);
            }
        }
    }
}
