using CategoryService.Contracts;
using CategoryService.Models;
using CategoryService.shared.MarkerInterface;
using Events.OccasionEvents;
using FluentValidation;
using MassTransit;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Occasions
{
    public static class DeleteOccasion
    {
        public sealed record DeleteOccasionCommand(int Id) : ICommand<Result<bool>>;

        public class Validator : AbstractValidator<DeleteOccasionCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Id).GreaterThan(0);
            }
        }
        internal sealed class Handler : IRequestHandler<DeleteOccasionCommand, Result<bool>>
        {
            private readonly IGenericRepository<Occasion, int> _occasionRepository;
            private readonly IValidator<DeleteOccasionCommand> _validator;



            public Handler(IGenericRepository<Occasion, int> occasionRepository, IValidator<DeleteOccasionCommand> validator)
            {
                _occasionRepository = occasionRepository;
                _validator = validator;
            }
            public async Task<Result<bool>> Handle(DeleteOccasionCommand request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure<bool>(new Error("DeleteOccasion.Validation", validationResult.ToString()));

                var occasion = await _occasionRepository.GetByIdAsync(request.Id);
                if (occasion == null)
                {
                    return Result.Failure<bool>(
                        new Error("NOT_FOUND", $"Occasion with ID '{request.Id}' was not found.")
                    );
                }

                bool hasProducts = await _occasionRepository.AnyAsync(
                o => o.Id == request.Id && o.ProductOccasions.Any(),
                cancellationToken);

                if (hasProducts)
                {
                    return Result.Failure<bool>(
                        new Error("Occasion.HasDependencies",
                            "Cannot delete occasion because it has assigned products."));
                }


                await _occasionRepository.DeleteAsync(request.Id);

                //await _publishEndpoint.Publish(new OccasionDeletedEvent
                //{
                //    OccasionId = occasion.Id,
                //    DeletedAt = DateTime.UtcNow,
                //}, cancellationToken);

                return Result.Success(true);
            }
        }

    }
}
