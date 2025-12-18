using CategoryService.Contracts;
using CategoryService.Contracts.Product;
using CategoryService.Models;
using CategoryService.shared.MarkerInterface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Products
{
    public static class GetBestSellerProducts
    {
        public sealed record Query(
        int Top = 10,
        int? CategoryId = null  
        ): ICommand<Result<List<GetProductsResponse>>>;

        internal sealed class Handler : IRequestHandler<Query, Result<List<GetProductsResponse>>>
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

            public async Task<Result<List<GetProductsResponse>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                // Build query
                var query = _productRepo
                    .GetAll(trackChanges: false)
                    .Include(p => p.Category)
                    .Include(p => p.ProductOccasions)
                        .ThenInclude(po => po.Occasion)
                    .Where(p => p.Status == Models.Enums.ProductStatus.InStock); // Active products only

                // Filter by category if specified
                if (request.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == request.CategoryId.Value);
                }

                // Get best sellers ordered by TotalSales and ViewCount
                var bestSellers = await query
                    .OrderByDescending(p => p.TotalSales)
                    .ThenByDescending(p => p.ViewCount)
                    .ThenByDescending(p => p.CreatedAtUtc)
                    .Take(request.Top)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        p.Discount,
                        p.ImageUrl,
                        p.Status,
                        p.TotalSales,
                        p.ViewCount,
                        CategoryName = p.Category!.Name,
                        OccasionNames = p.ProductOccasions
                            .Select(po => po.Occasion!.Name)
                            .ToList()
                    })
                    .ToListAsync(cancellationToken);

                // Map to response
                var response = bestSellers.Select(p => new GetProductsResponse(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Discount,
                    _fileService.GetFullUrl(p.ImageUrl),
                    p.Status.ToString(),
                    p.CategoryName,
                    p.OccasionNames
                )).ToList();

                return Result.Success(response);
            }
        }



    }
}
