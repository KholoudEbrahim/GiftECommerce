using FluentValidation;
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
            private readonly ILogger<Handler> _logger;

            public Handler(IGenericRepository<Stock, int> stockRepo,
            ILogger<Handler> logger)
            {
                _stockRepo = stockRepo;
                _logger = logger;
            }

            public async Task<Result<CheckStockAvailabilityResponse>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                // 1. Validation
                if (request.ProductId <= 0)
                    return Result.Failure<CheckStockAvailabilityResponse>(
                        new Error("CheckStock.Validation", "Valid product ID is required"));

                if (request.RequestedQuantity <= 0)
                    return Result.Failure<CheckStockAvailabilityResponse>(
                        new Error("CheckStock.Validation", "Requested quantity must be greater than 0"));

                _logger.LogInformation(
                    "Checking stock availability for Product {ProductId}, Quantity {Quantity}",
                    request.ProductId,
                    request.RequestedQuantity);

                // 2. Check stock
                var stock = await _stockRepo
                    .GetAll(s => s.ProductId == request.ProductId)
                    .FirstOrDefaultAsync(cancellationToken);

                // 3. Handle stock not found
                if (stock == null)
                {
                    _logger.LogWarning(
                        "Stock record not found for Product {ProductId}",
                        request.ProductId);

                    return Result.Success(new CheckStockAvailabilityResponse(
                        IsAvailable: false,
                        CurrentStock: 0,
                        RequestedQuantity: request.RequestedQuantity,
                        Message: "Product stock information not found"
                    ));
                }

                // 4. Determine availability
                var isAvailable = stock.CurrentStock >= request.RequestedQuantity;

                string message;
                if (isAvailable)
                {
                    message = "Stock available";
                    _logger.LogInformation(
                        "Stock available for Product {ProductId}: {Current}/{Requested}",
                        request.ProductId,
                        stock.CurrentStock,
                        request.RequestedQuantity);
                }
                else if (stock.CurrentStock == 0)
                {
                    message = "Product is out of stock";
                    _logger.LogWarning(
                        "Product {ProductId} is out of stock",
                        request.ProductId);
                }
                else
                {
                    message = $"Insufficient stock. Available: {stock.CurrentStock}, Requested: {request.RequestedQuantity}";
                    _logger.LogWarning(
                        "Insufficient stock for Product {ProductId}: Available={Available}, Requested={Requested}",
                        request.ProductId,
                        stock.CurrentStock,
                        request.RequestedQuantity);
                }


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
