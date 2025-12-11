using Contracts.OccasionEvents;
using MassTransit;
using MediatR;
using OccasionService.Data;
using Shared;

namespace OccasionService.Features.ToggleOccasionStatus
{
    public class ToggleOccasionStatusHandler : IRequestHandler<ToggleOccasionStatusCommand, Result>
    {
        private readonly OccasionRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;

        public ToggleOccasionStatusHandler(OccasionRepository repo, IPublishEndpoint publishEndpoint)
        {
            _repo = repo;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result> Handle(
        ToggleOccasionStatusCommand request,
        CancellationToken cancellationToken)
        {
            var occasion = await _repo.GetByIdAsync(request.Id);

            if (occasion == null)
            {
                return Result.Failure(
                    new Error("NOT_FOUND", $"Occasion with ID '{request.Id}' was not found.")
                );
            }

            occasion.IsActive = request.IsActive;

            _repo.Update(occasion);
            await _repo.SaveChangesAsync();

            await _publishEndpoint.Publish(new OccasionToggledStatusEvent
            {
                OccasionId = occasion.Id,
                IsActive = occasion.IsActive,
                ToggledAt = DateTime.UtcNow
            }, cancellationToken);

            return Result.Success();
        }
    }
}
