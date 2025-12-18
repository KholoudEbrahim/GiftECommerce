using InventoryService.Contracts;
using InventoryService.Contracts.Stock;
using InventoryService.Models;
using InventoryService.shared.MarkerInterface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace InventoryService.Features
{
    public static class GetStockInfo
    {
        public sealed record Query(int ProductId) : ICommand<Result<GetStockInfoResponse>>;

        internal sealed class Handler : IRequestHandler<Query, Result<GetStockInfoResponse>>
        {
            private readonly IGenericRepository<Stock, int> _stockRepo;

            public Handler(IGenericRepository<Stock, int> stockRepo)
            {
                _stockRepo = stockRepo;
            }

            public async Task<Result<GetStockInfoResponse>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var stock = await _stockRepo
                    .GetAll(s => s.ProductId == request.ProductId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (stock == null)
                    return Result.Failure<GetStockInfoResponse>(
                        new Error("Stock.NotFound",
                            $"Stock information for product {request.ProductId} not found"));

                var response = new GetStockInfoResponse(
                    stock.ProductId,
                    stock.ProductName,
                    stock.CurrentStock,
                    stock.MinStock,
                    stock.MaxStock,
                    stock.IsLowStock,
                    stock.IsOutOfStock,
                    stock.UpdatedAtUtc ?? stock.CreatedAtUtc
                );

                return Result.Success(response);
            }
        }

    }
}
