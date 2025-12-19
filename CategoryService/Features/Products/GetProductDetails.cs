using CategoryService.Contracts;
using CategoryService.Contracts.ExternalServices;
using CategoryService.Contracts.Product;
using CategoryService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Products
{
    public static class GetProductDetails
    {
        public sealed record Query(int Id) : IRequest<Result<GetProductDetailsResponse>>;

        internal sealed class Handler : IRequestHandler<Query, Result<GetProductDetailsResponse>>
        {
            private readonly IGenericRepository<Product, int> _productRepo;
            private readonly IFileStorageService _fileService;
            private readonly ISender _sender;
            private readonly IInventoryServiceClient _inventoryServiceClient;

            public Handler(
                IGenericRepository<Product, int> productRepo,
                IFileStorageService fileService,
                ISender sender,
                IInventoryServiceClient inventoryServiceClient)
            {
                _productRepo = productRepo;
                _fileService = fileService;
                _sender = sender;
                _inventoryServiceClient = inventoryServiceClient;
            }

            public async Task<Result<GetProductDetailsResponse>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                // Increment View Counts
                _ = Task.Run(async () =>
                {
                    var incrementCommand = new IncrementProductViewCount.Command(request.Id);
                    await _sender.Send(incrementCommand);
                }, cancellationToken);

                // Get product with all related data
                var product = await _productRepo
                    .GetAll(p => p.Id == request.Id, trackChanges: false)
                    .Include(p => p.Category)
                    .Include(p => p.ProductOccasions)
                        .ThenInclude(po => po.Occasion)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Price,
                        p.Discount,
                        p.ImageUrl,
                        p.Status,
                        p.Tags,
                        p.CreatedAtUtc,
                        Category = new
                        {
                            p.Category!.Id,
                            p.Category.Name,
                            p.Category.ImageUrl
                        },
                        Occasions = p.ProductOccasions.Select(po => new
                        {
                            po.Occasion!.Id,
                            po.Occasion.Name,
                            po.Occasion.ImageUrl
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (product == null)
                    return Result.Failure<GetProductDetailsResponse>(
                        new Error("Product.NotFound", $"Product with ID {request.Id} not found"));

                // Get stock info from InventoryService
                var stockInfo = await _inventoryServiceClient.GetStockInfoAsync(
                    request.Id,
                    cancellationToken);

                // Map to response
                var response = new GetProductDetailsResponse(
                    Id: product.Id,
                    Name: product.Name,
                    Description: product.Description,
                    Price: product.Price,
                    Discount: product.Discount,
                    ImageUrl: _fileService.GetFullUrl(product.ImageUrl),
                    Status: product.Status.ToString(),
                    Category: new CategoryDto(
                        product.Category.Id,
                        product.Category.Name,
                        _fileService.GetFullUrl(product.Category.ImageUrl)
                    ),
                    Occasions: product.Occasions.Select(o => new OccasionDto(
                        o.Id,
                        o.Name,
                        _fileService.GetFullUrl(o.ImageUrl)
                    )).ToList(),
                    Tags: string.IsNullOrWhiteSpace(product.Tags)
                        ? null
                        : product.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList(),
                    StockInfo: null,
                    CreatedAt: product.CreatedAtUtc
                );

                return Result.Success(response);
            }
        }
    }
}

