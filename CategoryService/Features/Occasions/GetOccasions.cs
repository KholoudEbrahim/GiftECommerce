using CategoryService.Contracts;
using CategoryService.Contracts.Category;
using CategoryService.Contracts.Occasion;
using CategoryService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Occasions
{
    public static class GetOccasions
    {
        public sealed record Query() : IRequest<Result<IEnumerable<GetOccasionsResponse>>>;

        internal sealed class Handler : IRequestHandler<Query, Result<IEnumerable<GetOccasionsResponse>>>
        {
            private readonly IGenericRepository<Occasion, int> _repository;
            private readonly IFileStorageService _fileService;

            public Handler(IGenericRepository<Occasion, int> repository, IFileStorageService imageService)
            {
                _repository = repository;
                _fileService = imageService;
            }
            public async Task<Result<IEnumerable<GetOccasionsResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var occasions = await _repository.GetAll(trackChanges: false)
               .Select(c => new
               {
                   c.Id,
                   c.Name,
                   c.ImageUrl,
                   c.Status
               })
               .ToListAsync(cancellationToken);
               
                var response = occasions.Select(c => new GetOccasionsResponse(
                c.Id,
                c.Name,
                _fileService.GetFullUrl(c.ImageUrl),
                c.Status.ToString()
            )).ToList();

                return Result.Success<IEnumerable<GetOccasionsResponse>>(response);
            }
        }

    }
}
