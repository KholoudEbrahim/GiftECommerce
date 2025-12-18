using InventoryService.Contracts;
using InventoryService.Contracts.StockTransaction;
using InventoryService.Models;
using InventoryService.shared.MarkerInterface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace InventoryService.Features
{
    public static class GetStockTransactions
    {
        public sealed record Query(
        int ProductId,
        int PageNumber = 1,
        int PageSize = 50
    ) : ICommand<Result<List<GetStockTransactionsResponse>>>;


        internal sealed class Handler : IRequestHandler<Query, Result<List<GetStockTransactionsResponse>>>
        {
            private readonly IGenericRepository<Stock, int> _stockRepo;
            private readonly IGenericRepository<StockTransaction, int> _transactionRepo;

            public Handler(
                IGenericRepository<Stock, int> stockRepo,
                IGenericRepository<StockTransaction, int> transactionRepo)
            {
                _stockRepo = stockRepo;
                _transactionRepo = transactionRepo;
            }

            public async Task<Result<List<GetStockTransactionsResponse>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                // Get stock record
                var stock = await _stockRepo
                    .GetAll(s => s.ProductId == request.ProductId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (stock == null)
                    return Result.Failure<List<GetStockTransactionsResponse>>(
                        new Error("Stock.NotFound",
                            $"Stock record for product {request.ProductId} not found"));

                // Get transactions
                var transactions = await _transactionRepo
                    .GetAll(t => t.StockId == stock.Id, trackChanges: false)
                    .OrderByDescending(t => t.CreatedAtUtc)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(t => new GetStockTransactionsResponse(
                        t.Id,
                        t.Type.ToString(),
                        t.Quantity,
                        t.StockBefore,
                        t.StockAfter,
                        t.Reference,
                        t.Notes,
                        t.PerformedBy,
                        t.CreatedAtUtc
                    ))
                    .ToListAsync(cancellationToken);

                return Result.Success(transactions);
            }
        }


    }
}
