using FluentValidation;
using MediatR;
using OfferService.Contracts;
using OfferService.Models;
using OfferService.shared.MarkerInterface;
using Shared.ApiResultResponse;

namespace OfferService.Features.DeleteOffer;

public static class DeleteOffer
{
    public sealed record Command(int Id) : ICommand<Result<bool>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
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

            await _repository.DeleteAsync(request.Id);

            return Result.Success(true);
        }
    }
}