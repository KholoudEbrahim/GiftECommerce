using MediatR;
using Shared;

namespace OccasionService.Features.UpdateOccasion
{
    public record UpdateOccasionCommand : IRequest<Result>
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public string? ImageUrl { get; init; }
    }

}
