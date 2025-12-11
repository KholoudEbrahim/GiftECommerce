using MediatR;
using Shared;

namespace OccasionService.Features.DeleteOccasion
{
    public class DeleteOccasionCommand : IRequest<Result>
    {
        public Guid Id { get; init; }
    }
}
