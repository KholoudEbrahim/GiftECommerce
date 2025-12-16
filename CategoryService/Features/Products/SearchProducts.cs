using CategoryService.Contracts;
using CategoryService.Contracts.Product;
using CategoryService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Products
{
    public static class SearchProducts
    {
        public sealed record Query(
        string? SearchTerm,
        decimal? MinPrice,
        decimal? MaxPrice,
        int? CategoryId,
        int? OccasionId,
        List<string>? Tags,
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<Result<PagedResult<GetProductsResponse>>>;

        internal sealed class Handler : IRequestHandler<Query, Result<PagedResult<GetProductsResponse>>>
        {
            private readonly IGenericRepository<Product, int> _productRepo;
            private readonly IFileStorageService _fileService;

            public Handler(
                IGenericRepository<Product, int> productRepo,
                IFileStorageService fileService)
            {
                _productRepo = productRepo;
                _fileService = fileService;
            }

            public async Task<Result<PagedResult<GetProductsResponse>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                // Build query
                var query = _productRepo.GetAll(trackChanges: false)
                    .Include(p => p.Category)
                    .Include(p => p.ProductOccasions)
                        .ThenInclude(po => po.Occasion)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchLower = request.SearchTerm.ToLower();
                    query = query.Where(p =>
                        p.Name.ToLower().Contains(searchLower) ||
                        p.Description.ToLower().Contains(searchLower) ||
                        (p.Tags != null && p.Tags.ToLower().Contains(searchLower))
                    );
                }

                if (request.MinPrice.HasValue)
                    query = query.Where(p => p.Price >= request.MinPrice.Value);

                if (request.MaxPrice.HasValue)
                    query = query.Where(p => p.Price <= request.MaxPrice.Value);

                if (request.CategoryId.HasValue)
                    query = query.Where(p => p.CategoryId == request.CategoryId.Value);

                if (request.OccasionId.HasValue)
                    query = query.Where(p => p.ProductOccasions.Any(po => po.OccasionId == request.OccasionId.Value));

                if (request.Tags != null && request.Tags.Any())
                {
                    foreach (var tag in request.Tags)
                    {
                        var tagLower = tag.ToLower();
                        query = query.Where(p => p.Tags != null && p.Tags.ToLower().Contains(tagLower));
                    }
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var products = await query
                    .OrderByDescending(p => p.CreatedAtUtc)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        p.Discount,
                        p.ImageUrl,
                        p.Status,
                        CategoryName = p.Category!.Name,
                        OccasionNames = p.ProductOccasions.Select(po => po.Occasion!.Name).ToList()
                    })
                    .ToListAsync(cancellationToken);

                // Map to response
                var response = products.Select(p => new GetProductsResponse(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Discount,
                    _fileService.GetFullUrl(p.ImageUrl),
                    p.Status.ToString(),
                    p.CategoryName,
                    p.OccasionNames
                )).ToList();

                var pagedResult = PagedResult<GetProductsResponse>.Create(
                    response,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                return Result.Success(pagedResult);
            }
        }
    }

}

