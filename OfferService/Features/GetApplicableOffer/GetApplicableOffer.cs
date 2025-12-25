using MediatR;
using Microsoft.EntityFrameworkCore;
using OfferService.Contracts;
using OfferService.Models;
using OfferService.Models.enums;
using Shared.ApiResultResponse;

namespace OfferService.Features;

public static class GetApplicableOffer
{
    public sealed record Query(int? ProductId, int? CategoryId, int? OccasionId) : IRequest<Result<GetApplicableOfferResponse>>;

    public sealed record GetApplicableOfferResponse(
        int OfferId,
        string OfferName,
        DiscountType Type,
        decimal Value
    );

    internal sealed class Handler : IRequestHandler<Query, Result<GetApplicableOfferResponse>>
    {
        private readonly IGenericRepository<Offer, int> _repository;

        public Handler(IGenericRepository<Offer, int> repository)
        {
            _repository = repository;
        }

        public async Task<Result<GetApplicableOfferResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var activeOffers = await _repository.GetAll(o =>
                o.IsActive &&
                !o.IsDeleted &&
                o.StartDateUtc <= now &&
                o.EndDateUtc >= now &&
                (
                   (o.ProductId == request.ProductId) ||
                   (o.CategoryId == request.CategoryId) ||
                   (request.OccasionId.HasValue && o.OccasionId == request.OccasionId.Value)
                )
            ).ToListAsync(cancellationToken);

            if (!activeOffers.Any())
            {
                return Result.Failure<GetApplicableOfferResponse>(new Error("Offer.None", "No active offers found."));
            }

            
            var bestOffer = activeOffers
                .OrderByDescending(o => o.ProductId.HasValue) // Priority 1: Product
                .ThenByDescending(o => o.CategoryId.HasValue) // Priority 2: Category
                .ThenByDescending(o => o.Value)               // Priority 3: Highest Value
                .First();

            return Result.Success(new GetApplicableOfferResponse(
                bestOffer.Id,
                bestOffer.Name,
                bestOffer.Type,
                bestOffer.Value                
            ));
        }
    }
}