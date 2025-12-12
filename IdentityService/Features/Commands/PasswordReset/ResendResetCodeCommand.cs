using IdentityService.Data;
using IdentityService.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Commands.PasswordReset
{
    public record ResendResetCodeCommand(string Email) : IRequest<RequestResponse<RequestPasswordResetResponseDto>>;

    public class ResendResetCodeCommandHandler
        : IRequestHandler<ResendResetCodeCommand, RequestResponse<RequestPasswordResetResponseDto>>
    {
        private readonly IRequestHandler<RequestPasswordResetCommand, RequestResponse<RequestPasswordResetResponseDto>> _resetHandler;
        private readonly IdentityDbContext _context;
        private readonly ILogger<ResendResetCodeCommandHandler> _logger;

        public ResendResetCodeCommandHandler(
            IRequestHandler<RequestPasswordResetCommand, RequestResponse<RequestPasswordResetResponseDto>> resetHandler,
            IdentityDbContext context,
            ILogger<ResendResetCodeCommandHandler> logger)
        {
            _resetHandler = resetHandler;
            _context = context;
            _logger = logger;
        }

        public async Task<RequestResponse<RequestPasswordResetResponseDto>> Handle(
            ResendResetCodeCommand request,
            CancellationToken cancellationToken)
        {
            try
            {

                var previousRequests = await _context.PasswordResetRequests
                    .Where(r => r.Email.ToLower() == request.Email.ToLower()
                             && !r.IsUsed
                             && r.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var req in previousRequests)
                {
                    req.IsUsed = true;
                    req.UsedAt = DateTime.UtcNow;
                    req.UpdatedAt = DateTime.UtcNow;
                }

                if (previousRequests.Any())
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }

 
                var resetCommand = new RequestPasswordResetCommand(request.Email);
                var result = await _resetHandler.Handle(resetCommand, cancellationToken);

                _logger.LogInformation("Reset code resent for {Email}", request.Email);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending reset code for {Email}", request.Email);
                return RequestResponse<RequestPasswordResetResponseDto>.Fail(
                    "An error occurred while resending the code",
                    500);
            }
        }
    }
}
