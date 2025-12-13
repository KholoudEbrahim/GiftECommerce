using CategoryService.Contracts;
using CategoryService.Contracts.Category;
using CategoryService.Contracts.Occasion;
using CategoryService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Occasions
{
    public static class GetOccasionDetails
    {
        public sealed record Query(int Id) : IRequest<Result<GetOccasionDetailsResponse>>;

        internal sealed class Handler : IRequestHandler<Query, Result<GetOccasionDetailsResponse>>
        {
            private readonly IGenericRepository<Occasion, int> _repository;
            private readonly IFileStorageService _fileService;

            public Handler(IGenericRepository<Occasion, int> repository, IFileStorageService fileService)
            {
                _repository = repository;
                _fileService = fileService;
            }

            public async Task<Result<GetOccasionDetailsResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var data = await _repository.GetAll(c => c.Id == request.Id)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.ImageUrl,
                        c.Status,
                        Products = c.ProductOccasions.Select(p => new
                        {
                            p.Product.Id,
                            p.Product.Name,
                            p.Product.Price,
                            p.Product.Discount,
                            p.Product.ImageUrl,
                            p.Product.Status
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (data is null)
                {
                    return Result.Failure<GetOccasionDetailsResponse>(new Error("Occasion.NotFound", "Occasion not found"));
                }

                // B. Map URLs (In Memory)
                var response = new GetOccasionDetailsResponse(
                    data.Id,
                    data.Name,
                    _fileService.GetFullUrl(data.ImageUrl),
                    data.Status.ToString(),
                    data.Products.Select(p => new OccasionProductDto(
                        p.Id,
                        p.Name,
                        p.Price,
                        p.Discount,
                        _fileService.GetFullUrl(p.ImageUrl),
                        p.Status.ToString(),
                        ""//CategoryName
                    )).ToList()
                );

                return Result.Success(response);
            }
        }
    }
}
