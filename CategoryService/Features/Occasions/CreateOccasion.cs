using CategoryService.Contracts;
using Events.OccasionEvents;
using CategoryService.Models;
using CategoryService.Models.Enums;
using FluentValidation;
using MassTransit;
using MediatR;
using Shared.ApiResultResponse;
using CategoryService.shared.MarkerInterface;

namespace CategoryService.Features.Occasions
{
    public static class CreateOccasion
    {
        public sealed record CreateOccasionCommand (string Name, string? ImageUrl, bool IsActive) : ICommand<Result<int>>;

        

        public class CreateOccasionValidator : AbstractValidator<CreateOccasionCommand>
        {
            public CreateOccasionValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Occasion name is required.");

                RuleFor(x => x.Name)
                    .Length(3, 100)
                    .WithMessage("Occasion name must be between 3 and 100 characters");

                RuleFor(x => x.ImageUrl)
                   .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                   .When(x => !string.IsNullOrEmpty(x.ImageUrl))
                   .WithMessage("Invalid image URL format");

            }
        }

        internal sealed class CreateOccasionHandler : IRequestHandler<CreateOccasionCommand, Result<int>>
        {
            private readonly IGenericRepository<Occasion,int> _repo;
            private readonly IValidator<CreateOccasionCommand> _validator;
            private readonly IPublishEndpoint _publishEndpoint;

            public CreateOccasionHandler(IGenericRepository<Occasion,int> repo, IPublishEndpoint publishEndpoint, IValidator validator)
            {
                _repo = repo;
                _publishEndpoint = publishEndpoint;
                _validator = (IValidator<CreateOccasionCommand>?)validator;
            }

            public async Task<Result<int>> Handle(CreateOccasionCommand request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure<int>(new Error("CreateOccasion.Validation", validationResult.ToString()));

                var existingOccasion = await _repo.AnyAsync(c => c.Name == request.Name, cancellationToken);

                if (existingOccasion)
                {
                    return Result.Failure<int>(
                        new Error("Occasion.NameNotUnique", $"The occasion name '{request.Name}' already exists.")
                        );
                }


                var newOccasion = new CategoryService.Models.Occasion
                {
                    Name = request.Name,
                    Status = request.IsActive ? OccasionStatus.Active : OccasionStatus.InActive,
                    ImageUrl = request.ImageUrl,
                    IsDeleted = false,
                };


                await _repo.AddAsync(newOccasion);
                await _repo.SaveChangesAsync();

                await _publishEndpoint.Publish(new OccasionCreatedEvent
                {
                    OccasionId = newOccasion.Id,
                    Name = newOccasion.Name,
                    IsActive = newOccasion.Status == OccasionStatus.Active,
                    CreatedAt = newOccasion.CreatedAtUtc,
                }, cancellationToken);

                return Result.Success(newOccasion.Id);

            }
        }

    }
}
