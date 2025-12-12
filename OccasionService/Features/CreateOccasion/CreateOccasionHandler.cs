using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OccasionService.Data;
using OccasionService.Models;
using Shared.ApiResultResponse;

namespace OccasionService.Features.CreateOccasion
{
    public class CreateOccasionHandler : IRequestHandler<CreateOccasionCommand, Result<CreateOccasionRequest>>
    {
        private readonly OccasionRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateOccasionHandler(OccasionRepository repo, IPublishEndpoint publishEndpoint)
        {
            _repo = repo;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<CreateOccasionRequest>> Handle(CreateOccasionCommand request, CancellationToken cancellationToken)
        {
            var existingOccasion = await _repo.ExistsByNameAsync(request.Name);

            if (existingOccasion)
            {
                return Result.Failure<CreateOccasionRequest>(
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
            await _repo.SaveChangesAsync();

            //await _publishEndpoint.Publish(new OccasionCreatedEvent
            //{
            //    OccasionId = newOccasion.Id,
            //    Name = newOccasion.Name,
            //    IsActive = newOccasion.IsActive,
            //    CreatedAt = newOccasion.CreatedAt
            //}, cancellationToken);

            return Result.Success(new CreateOccasionRequest
            {
                Id = newOccasion.Id,
                Name = newOccasion.Name,
                IsActive = newOccasion.IsActive,
                CreatedAt = newOccasion.CreatedAtUtc
            });

        }
    }
}
