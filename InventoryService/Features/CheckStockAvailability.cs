using InventoryService.Contracts;
using InventoryService.Contracts.Stock;
using InventoryService.Models;
using InventoryService.shared.MarkerInterface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace InventoryService.Features
{
    // For Cart Service to check stock availability before placing an order
    public static class CheckStockAvailability
    {
        public sealed record Query(
        int ProductId,
        int RequestedQuantity
    ) : ICommand<Result<CheckStockAvailabilityResponse>>;


        internal sealed class Handler : IRequestHandler<Query, Result<CheckStockAvailabilityResponse>>
        {
            private readonly IGenericRepository<Stock, int> _stockRepo;

            public Handler(IGenericRepository<Stock, int> stockRepo)
            {
                _stockRepo = stockRepo;
            }

            public async Task<Result<CheckStockAvailabilityResponse>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var stock = await _stockRepo
                    .GetAll(s => s.ProductId == request.ProductId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (stock == null)
                {
                    return Result.Success(new CheckStockAvailabilityResponse(
                        false,
                        0,
                        request.RequestedQuantity,
                        "Product stock information not found"
                    ));
                }

                var isAvailable = stock.CurrentStock >= request.RequestedQuantity;
                var message = isAvailable
                    ? "Stock available"
                    : $"Insufficient stock. Available: {stock.CurrentStock}, Requested: {request.RequestedQuantity}";

                return Result.Success(new CheckStockAvailabilityResponse(
                    isAvailable,
                    stock.CurrentStock,
                    request.RequestedQuantity,
                    message
                ));
            }
        }


    }
}
