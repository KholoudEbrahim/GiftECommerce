using CategoryService.Contracts;
using CategoryService.Contracts.Category;
using CategoryService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Categories;

public static class GetCategoryDetails
{
    public sealed record Query(int Id) : IRequest<Result<GetCategoryDetailsResponse>>;

  

   
    internal sealed class Handler : IRequestHandler<Query, Result<GetCategoryDetailsResponse>>
    {
        private readonly IGenericRepository<Category, int> _repository;
        private readonly IFileStorageService _fileService;

        public Handler(IGenericRepository<Category, int> repository, IFileStorageService fileService)
        {
            _repository = repository;
            _fileService = fileService;
        }

        public async Task<Result<GetCategoryDetailsResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAll(c => c.Id == request.Id)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.ImageUrl,
                    c.Status,
                    Products = c.Products.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        p.Discount,
                        p.ImageUrl,
                        p.Status
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data is null)
            {
                return Result.Failure<GetCategoryDetailsResponse>(new Error("Category.NotFound", "Category not found"));
            }

            // B. Map URLs (In Memory)
            var response = new GetCategoryDetailsResponse(
                data.Id,
                data.Name,
                _fileService.GetFullUrl(data.ImageUrl),
                data.Status.ToString(),
                data.Products.Select(p => new ProductDto(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Discount,
                    _fileService.GetFullUrl(p.ImageUrl), 
                    p.Status.ToString() 
                )).ToList()
            );

            return Result.Success(response);
        }
    }
}