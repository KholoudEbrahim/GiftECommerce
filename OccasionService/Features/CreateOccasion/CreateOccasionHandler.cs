using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OccasionService.Data;
using OccasionService.Models;
using Shared;

namespace OccasionService.Features.CreateOccasion
{
    public class CreateOccasionHandler : IRequestHandler<CreateOccasionCommand, Result<Guid>>
    {
        private readonly OccasionRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateOccasionHandler(OccasionRepository repo, IPublishEndpoint publishEndpoint)
        {
            _repo = repo;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<Guid>> Handle(CreateOccasionCommand request, CancellationToken cancellationToken)
        {
            var existingOccasion = await _repo.ExistsByNameAsync(request.Name);

            if (existingOccasion)
            {
                return Result.Failure<Guid>(
                    new Error("OccasionAlreadyExists", "An occasion with the same name already exists.")
                    );
            }

            var newOccasion = new OccasionService.Models.Occasion
            {
                Name = request.Name,
                IsActive = request.IsActive,
                ImageUrl = request.ImageUrl
            };

            await _repo.AddAsync(newOccasion);

            return Result.Success(newOccasion.Id);

        }
    }
}
