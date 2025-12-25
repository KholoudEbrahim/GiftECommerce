using MediatR;
using Microsoft.EntityFrameworkCore;
using OfferService.Contracts;
using OfferService.Models;
using Shared.ApiResultResponse;
namespace OfferService.Features.GetAllOffers;


public static class GetAllOffers
{
    public sealed record Query() : IRequest<Result<IEnumerable<GetAllOffersResponse>>>;

    public sealed record GetAllOffersResponse(
        int Id,
        string Name,
        string Description,
        string Type,
        decimal Value,
        DateTime StartDate,
        DateTime EndDate,
        string Status, 
        string Target  
    );

    internal sealed class Handler : IRequestHandler<Query, Result<IEnumerable<GetAllOffersResponse>>>
    {
        private readonly IGenericRepository<Offer, int> _repository;

        public Handler(IGenericRepository<Offer, int> repository)
        {
            _repository = repository;
        }

        public async Task<Result<IEnumerable<GetAllOffersResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var offers = await _repository.GetAll(trackChanges: false)
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.StartDateUtc)
                .ToListAsync(cancellationToken);

            var GetAllOffersresponse = offers.Select(o => new GetAllOffersResponse(
                o.Id,
                o.Name,
                o.Description,
                o.Type.ToString(),
                o.Value,
                o.StartDateUtc,
                o.EndDateUtc,
                GetStatus(o),
                GetTargetDescription(o)
            ));

            return Result.Success(GetAllOffersresponse);
        }

        private static string GetStatus(Offer o)
        {
            if (!o.IsActive) return "Disabled";
            if (DateTime.UtcNow > o.EndDateUtc) return "Expired";
            if (DateTime.UtcNow < o.StartDateUtc) return "Scheduled";
            return "Active";
        }

        private static string GetTargetDescription(Offer o)
        {
            if (o.ProductId.HasValue) return $"Product ID: {o.ProductId}";
            if (o.CategoryId.HasValue) return $"Category ID: {o.CategoryId}";
            if (o.OccasionId.HasValue) return $"Occasion ID: {o.OccasionId}";
            return "Global";
        }
    }
}