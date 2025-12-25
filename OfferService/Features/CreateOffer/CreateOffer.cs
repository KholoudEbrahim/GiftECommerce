using FluentValidation;
using MediatR;
using OfferService.Contracts;
using OfferService.Models;
using OfferService.Models.enums;
using OfferService.shared.MarkerInterface;
using Shared.ApiResultResponse;

namespace OfferService.Features;

public static class CreateOffer
{
    public sealed record Command(
        string Name,
        string Description,
        DiscountType Type,
        decimal Value,
        DateTime StartDate,
        DateTime EndDate,
        int? ProductId,
        int? CategoryId,
        int? OccasionId
    ) : ICommand<Result<int>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Value).GreaterThan(0);

            // Percentage should not exceed 100%
            RuleFor(x => x.Value).LessThanOrEqualTo(100)
                .When(x => x.Type == DiscountType.Percentage)
                .WithMessage("Percentage discount cannot exceed 100%.");

            // Date Validation
            RuleFor(x => x.StartDate).LessThan(x => x.EndDate)
                .WithMessage("Start date must be before end date.");

            // Targeting Validation: Must target at least SOMETHING (optional rule)
            RuleFor(x => x)
                .Must(x => x.ProductId.HasValue || x.CategoryId.HasValue || x.OccasionId.HasValue)
                .WithMessage("An offer must apply to a Product, Category, or Occasion.");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<int>>
    {
        private readonly IGenericRepository<Offer, int> _repository;
        private readonly IValidator<Command> _validator;

        public Handler(IGenericRepository<Offer, int> repository , IValidator<Command> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            var ValidationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!ValidationResult.IsValid)
            {
                return Result.Failure<int>(new Error("CreateOffer.Validation" , ValidationResult.ToString()));
            }


            var isActiveOfferExists = await _repository.AnyAsync(o =>
                o.IsActive &&
                (
                    (request.ProductId.HasValue && o.ProductId == request.ProductId) ||
                    (request.CategoryId.HasValue && o.CategoryId == request.CategoryId) ||
                    (request.OccasionId.HasValue && o.OccasionId == request.OccasionId)
                ) &&
                (
                    (request.StartDate < o.EndDateUtc && request.EndDate > o.StartDateUtc)
                ), cancellationToken);

            if (isActiveOfferExists)
            {
                return Result.Failure<int>(new Error("CreateOffer.Conflict", "An active offer already exists for the specified target within the given date range."));
            }


            var offer = new Offer
            {
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                Value = request.Value,
                StartDateUtc = request.StartDate.ToUniversalTime(),
                EndDateUtc = request.EndDate.ToUniversalTime(),
                ProductId = request.ProductId,
                CategoryId = request.CategoryId,
                OccasionId = request.OccasionId,
                IsActive = true
            };

            await _repository.AddAsync(offer);
            
            return Result.Success(offer.Id);
        }
    }
}