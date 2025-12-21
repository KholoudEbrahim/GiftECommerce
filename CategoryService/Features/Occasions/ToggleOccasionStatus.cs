using CategoryService.Contracts;
using CategoryService.Models;
using CategoryService.Models.Enums;
using Events.OccasionEvents;
using FluentValidation;
using MassTransit;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Occasions
{
    public static class ToggleOccasionStatus
    {
        public sealed record ToggleOccasionStatusCommand(int Id, bool IsActive) : IRequest<Result<int>>;

        public class ToggleOccasionStatusValidator : AbstractValidator<ToggleOccasionStatusCommand>
        {
            public ToggleOccasionStatusValidator()
            {
                RuleFor(x => x.Id)
               .NotEmpty()
               .WithMessage("Occasion ID is required");
            }
        }

        internal sealed class Handler : IRequestHandler<ToggleOccasionStatusCommand, Result<int>>
        {
            private readonly IGenericRepository<Occasion, int> _repository;
            private readonly IValidator<ToggleOccasionStatusCommand> _validator;


            public Handler(IGenericRepository<Occasion, int> repository, IValidator<ToggleOccasionStatusCommand> validator)
            {
                _repository = repository;
                _validator = validator;
            }
 
            public async Task<Result<int>> Handle(ToggleOccasionStatusCommand request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure<int>(new Error("ToggleOccasionStatus.Validation", validationResult.ToString()));


                var occasion = await _repository.GetByIdAsync(request.Id);
                if (occasion is null)
                {
                    return Result.Failure<int>(new Error("Occasion.NotFound", "Occasion not found"));
                }

                // Toggle Status
                var status = occasion.Status = request.IsActive? OccasionStatus.Active : OccasionStatus.InActive;

                string[] UpdatedValues = [nameof(occasion.Status)];
                _repository.SaveInclude(occasion, UpdatedValues);

                return Result.Success(occasion.Id);
            }
        }
    }
}
