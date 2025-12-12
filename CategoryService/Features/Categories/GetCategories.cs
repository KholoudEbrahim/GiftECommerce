using CategoryService.Contracts;
using CategoryService.Contracts.Category;
using CategoryService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Categories;


public static class GetCategories
{
    public sealed record Query() : IRequest<Result<IEnumerable<GetCategoriesResponse>>>;

   

    
    internal sealed class Handler : IRequestHandler<Query, Result<IEnumerable<GetCategoriesResponse>>>
    {
        private readonly IGenericRepository<Category, int> _repository;
        private readonly IFileStorageService _fileService;

        public Handler(IGenericRepository<Category, int> repository, IFileStorageService imageService)
        {
            _repository = repository;
            _fileService = imageService;
        }

        public async Task<Result<IEnumerable<GetCategoriesResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var categories = await _repository.GetAll(trackChanges: false)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.ImageUrl,
                    c.Status
                })
                .ToListAsync(cancellationToken);

            var response = categories.Select(c => new GetCategoriesResponse(
                c.Id,
                c.Name,
                _fileService.GetFullUrl(c.ImageUrl), 
                c.Status.ToString()
            )).ToList();

            return Result.Success<IEnumerable<GetCategoriesResponse>>(response);
        }
    }
}
