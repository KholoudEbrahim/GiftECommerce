using FluentValidation;
using MediatR;
using OfferService.Contracts;
using OfferService.Models;
using OfferService.shared.MarkerInterface;
using Shared.ApiResultResponse;

namespace OfferService.Features.UpdateOffer;

public static class UpdateOffer
{
    public sealed record Command(
        int Id,
        string Name,
        string Description,
        decimal Value,
        DateTime StartDate,
        DateTime EndDate,
        bool IsActive
    ) : ICommand<Result<bool>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Value).GreaterThan(0);
            RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<bool>>
    {
        private readonly IGenericRepository<Offer, int> _repository;

        public Handler(IGenericRepository<Offer, int> repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var offer = await _repository.GetByIdAsync(request.Id);

            if (offer is null)
                return Result.Failure<bool>(new Error("Offer.NotFound", $"Offer with Id {request.Id} was not found."));

            offer.Name = request.Name;
            offer.Description = request.Description;
            offer.Value = request.Value;
            offer.StartDateUtc = request.StartDate.ToUniversalTime();
            offer.EndDateUtc = request.EndDate.ToUniversalTime();
            offer.IsActive = request.IsActive;

            _repository.Update(offer);

            return Result.Success(true);
        }
    }
}