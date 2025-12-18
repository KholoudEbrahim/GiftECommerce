using InventoryService.Contracts;
using InventoryService.Contracts.Stock;
using InventoryService.Models;
using InventoryService.shared.MarkerInterface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace InventoryService.Features
{
    public static class GetAllStocks
    {
        public sealed record Query(
        bool? OnlyLowStock = null
    ) : ICommand<Result<List<GetStockInfoResponse>>>;


        internal sealed class Handler : IRequestHandler<Query, Result<List<GetStockInfoResponse>>>
        {
            private readonly IGenericRepository<Stock, int> _stockRepo;

            public Handler(IGenericRepository<Stock, int> stockRepo)
            {
                _stockRepo = stockRepo;
            }

            public async Task<Result<List<GetStockInfoResponse>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var query = _stockRepo.GetAll(trackChanges: false);

                // Filter by low stock if requested
                if (request.OnlyLowStock == true)
                {
                    query = query.Where(s => s.CurrentStock <= s.MinStock);
                }

                var stocks = await query
                    .OrderBy(s => s.CurrentStock)
                    .Select(s => new GetStockInfoResponse(
                        s.ProductId,
                        s.ProductName,
                        s.CurrentStock,
                        s.MinStock,
                        s.MaxStock,
                        s.IsLowStock,
                        s.IsOutOfStock,
                        s.UpdatedAtUtc ?? s.CreatedAtUtc
                    ))
                    .ToListAsync(cancellationToken);

                return Result.Success(stocks);
            }
        }


    }
}
