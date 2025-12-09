using System;
using MediatR;
using Shared;


namespace OccasionService.Features.CreateOccasion
{
    public record CreateOccasionCommand : IRequest<Result<Guid>>
    {
        public string Name { get; init; }
        public bool IsActive { get; init; } = true;
        public string ImageUrl { get; init; }

    }

    public record CreateOccasionResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
