using IdentityService.Data;
using IdentityService.Features.Commands.PasswordReset.RequestPasswordReset;
using IdentityService.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Commands.PasswordReset.ResendResetCode
{
    public record ResendResetCodeCommand(string Email) : IRequest<RequestResponse<RequestPasswordResetResponseDto>>;

    public class ResendResetCodeCommandHandler
       : IRequestHandler<ResendResetCodeCommand, RequestResponse<RequestPasswordResetResponseDto>>
    {
        private readonly IRepository _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<ResendResetCodeCommandHandler> _logger;

        public ResendResetCodeCommandHandler(
            IRepository repository,
            ILogger<ResendResetCodeCommandHandler> logger,
            IMediator mediator)
        {
            _repository = repository;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<RequestResponse<RequestPasswordResetResponseDto>> Handle(
            ResendResetCodeCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
           
                await _repository.InvalidatePasswordResetRequestsAsync(request.Email);

          
                var resetCommand = new RequestPasswordResetCommand(request.Email);
                var result = await _mediator.Send(resetCommand, cancellationToken);

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
