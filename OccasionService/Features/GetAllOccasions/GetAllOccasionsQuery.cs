using MediatR;
using OccasionService.Data;
using OccasionService.Models;
using Shared;
using Shared.ApiResultResponse;

namespace OccasionService.Features.GetAllOccasions
{
    public record GetAllOccasionsQuery : IRequest<Result<PagedResult<OccasionDto>>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public bool? IsActive { get; init; }
        public string SearchTerm { get; init; }



    }
}
