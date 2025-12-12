using MediatR;
using Shared.ApiResultResponse;

namespace OccasionService.Features.ToggleOccasionStatus
{
    public record ToggleOccasionStatusCommand : IRequest<Result>
    {
        public Guid Id { get; init; }
        public bool IsActive { get; init; }

    }

}
