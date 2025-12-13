using CategoryService.Contracts;
using CategoryService.Models;
using CategoryService.Models.Enums;
using CategoryService.shared.MarkerInterface;
using Events.OccasionEvents;
using FluentValidation;
using MassTransit;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Occasions
{
    public static class UpdateOccasion
    {
        public sealed record UpdateOccasionCommand(int Id, string Name, string? ImageUrl, bool IsActive) : ICommand<Result<bool>>;

        public class UpdateOccasionValidator : AbstractValidator<UpdateOccasionCommand>
        {
            public UpdateOccasionValidator()
            {
                RuleFor(x => x.Id)
               .NotEmpty()
               .WithMessage("Occasion ID is required");

                RuleFor(x => x.Name)
               .NotEmpty()
               .WithMessage("Occasion name is required");

                RuleFor(x => x.Name)
                .Length(3, 100)
                .WithMessage("Occasion name must be between 3 and 100 characters");

                RuleFor(x => x.ImageUrl)
                .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.ImageUrl))
                .WithMessage("Invalid image URL format");
            }
        }

        internal sealed class UpdateOccasionHandler : IRequestHandler<UpdateOccasionCommand, Result<bool>>
        {
            private readonly IGenericRepository<Occasion, int> _repository;
            private readonly IValidator<UpdateOccasionCommand> _validator;
            private readonly IPublishEndpoint _publishEndpoint;

            public UpdateOccasionHandler(IGenericRepository<Occasion, int> repository, IValidator<UpdateOccasionCommand> validator, IPublishEndpoint publishEndpoint)
            {
                _repository = repository;
                _validator = validator;
                _publishEndpoint = publishEndpoint;
            }

            public async Task<Result<bool>> Handle(UpdateOccasionCommand request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure<bool>(new Error("UpdateOccasion.Validation", validationResult.ToString()));


                var occasion = await _repository.GetByIdAsync(request.Id, trackChanges: true);

                if (occasion == null)
                {
                    return Result.Failure<bool>(
                        new Error("NOT_FOUND", $"Occasion with ID '{request.Id}' was not found.")
                    );
                }

                var duplicateExists = await _repository.AnyAsync(c => c.Name == request.Name && c.Id != request.Id, cancellationToken);

                if (duplicateExists && occasion.Name != request.Name)
                {
                    return Result.Failure<bool>(
                        new Error("DUPLICATE_NAME", $"An occasion with the name '{request.Name}' already exists.")
                    );
                }

                occasion.Name = request.Name;
                occasion.Status = request.IsActive ? OccasionStatus.Active : OccasionStatus.InActive;
                occasion.ImageUrl = request.ImageUrl;

                string[] UpdatedValues = [nameof(occasion.Name), nameof(occasion.ImageUrl), nameof(occasion.Status)];
                _repository.SaveInclude(occasion, UpdatedValues);

                await _publishEndpoint.Publish(new OccasionUpdatedEvent
                {
                    OccasionId = occasion.Id,
                    Name = occasion.Name,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = occasion.Status == OccasionStatus.Active,
                }, cancellationToken);


                return Result.Success(true);
            }
        }
    }
}
