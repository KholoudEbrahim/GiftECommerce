using MassTransit;
using MediatR;
using OccasionService.Data;
using Shared.ApiResultResponse;

namespace OccasionService.Features.DeleteOccasion
{
    public class DeleteOccasionHandler : IRequestHandler<DeleteOccasionCommand, Result>
    {
        private readonly OccasionRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;

        public DeleteOccasionHandler(OccasionRepository repo, IPublishEndpoint publishEndpoint)
        {
            _repo = repo;
            _publishEndpoint = publishEndpoint;
        }
        public async Task<Result> Handle(DeleteOccasionCommand request, CancellationToken cancellationToken)
        {
            var occasion = await _repo.GetByIdAsync(request.Id);
            if (occasion == null)
            {
                return Result.Failure(
                    new Error("NOT_FOUND", $"Occasion with ID '{request.Id}' was not found.")
                );
            }
            _repo.Delete(occasion);
            await _repo.SaveChangesAsync();

            //await _publishEndpoint.Publish(new OccasionDeletedEvent
            //{
            //    OccasionId = occasion.Id,
            //    DeletedAt = DateTime.UtcNow,
            //}, cancellationToken);


            return Result.Success();
        }
    }
}
