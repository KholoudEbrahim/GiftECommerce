using MassTransit;
using MediatR;
using OccasionService.Data;
using Shared.ApiResultResponse;

namespace OccasionService.Features.UpdateOccasion;

public class UpdateOccasionHandler : IRequestHandler<UpdateOccasionCommand, Result>
{
    private readonly OccasionRepository _repo;
    private readonly IPublishEndpoint _publishEndpoint;


    public UpdateOccasionHandler(OccasionRepository repo, IPublishEndpoint publishEndpoint)
    {
        _repo = repo;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(UpdateOccasionCommand request, CancellationToken cancellationToken)
    {
        var occasion = await _repo.GetByIdAsync(request.Id);

        if (occasion == null)
        {
            return Result.Failure(
                new Error("NOT_FOUND", $"Occasion with ID '{request.Id}' was not found.")
            );
        }

        var duplicateExists = await _repo.ExistsByNameAsync(request.Name);

        if (duplicateExists && occasion.Name != request.Name)
        {
            return Result.Failure(
                new Error("DUPLICATE_NAME", $"An occasion with the name '{request.Name}' already exists.")
            );
        }

        occasion.Name = request.Name;
        occasion.IsActive = request.IsActive;
        occasion.ImageUrl = request.ImageUrl;

        _repo.Update(occasion);
        await _repo.SaveChangesAsync();

        //await _publishEndpoint.Publish(new OccasionUpdatedEvent
        //{
        //    OccasionId = occasion.Id,
        //    Name = occasion.Name,
        //    IsActive = occasion.IsActive,
        //    UpdatedAt = occasion.UpdatedAt.Value
        //}, cancellationToken);


        return Result.Success();
    }
}
