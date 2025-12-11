using System;
using MediatR;
using Shared;


namespace OccasionService.Features.CreateOccasion
{
    public record CreateOccasionCommand : IRequest<Result<CreateOccasionRequest>>
    {
        public string Name { get; init; }
        public bool IsActive { get; init; } = true;
        public string? ImageUrl { get; init; }

    }

    
}
