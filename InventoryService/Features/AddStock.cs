using Events.InventoryEvents;
using FluentValidation;
using InventoryService.Contracts;
using InventoryService.Contracts.Stock;
using InventoryService.Models;
using InventoryService.Models.Enums;
using InventoryService.shared.MarkerInterface;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace InventoryService.Features
{
    public static class AddStock
    {
        public sealed record Command(
        int ProductId,
        int Quantity,
        string? Notes,
        string? PerformedBy
    ) : ICommand<Result<AddStockResponse>>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.ProductId)
                    .GreaterThan(0)
                    .WithMessage("Valid product ID is required");

                RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Quantity must be greater than 0");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<AddStockResponse>>
        {
            private readonly IGenericRepository<Stock, int> _stockRepo;
            private readonly IGenericRepository<StockTransaction, int> _transactionRepo;
            private readonly IValidator<Command> _validator;
            private readonly IPublishEndpoint _publishEndpoint;

            public Handler(
                IGenericRepository<Stock, int> stockRepo,
                IGenericRepository<StockTransaction, int> transactionRepo,
                IValidator<Command> validator,
                IPublishEndpoint publishEndpoint)
            {
                _stockRepo = stockRepo;
                _transactionRepo = transactionRepo;
                _validator = validator;
                _publishEndpoint = publishEndpoint;
            }

            public async Task<Result<AddStockResponse>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                // 1. Validation
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure<AddStockResponse>(
                        new Error("AddStock.Validation", validationResult.ToString()));

                // 2. Get Stock Record
                var stock = await _stockRepo
                    .GetAll(s => s.ProductId == request.ProductId, trackChanges: true)
                    .FirstOrDefaultAsync(cancellationToken);

                if (stock == null)
                    return Result.Failure<AddStockResponse>(
                        new Error("Stock.NotFound",
                            $"Stock record for product {request.ProductId} not found. " +
                            "Please set min/max stock first."));

                // 3. Check if new stock exceeds MaxStock
                var newStock = stock.CurrentStock + request.Quantity;
                if (newStock > stock.MaxStock)
                    return Result.Failure<AddStockResponse>(
                        new Error("Stock.ExceedsMax",
                            $"Adding {request.Quantity} units would exceed maximum stock of {stock.MaxStock}. " +
                            $"Current: {stock.CurrentStock}, Max: {stock.MaxStock}"));

                // 4. Record the transaction BEFORE updating stock
                var transaction = new StockTransaction
                {
                    StockId = stock.Id,
                    Type = StockTransactionType.StockAdded,
                    Quantity = request.Quantity,
                    StockBefore = stock.CurrentStock,
                    StockAfter = newStock,
                    Notes = request.Notes,
                    PerformedBy = request.PerformedBy ?? "System",
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _transactionRepo.AddAsync(transaction);

                // 5. Update Stock
                stock.CurrentStock = newStock;
                stock.UpdatedAtUtc = DateTime.UtcNow;
                _stockRepo.Update(stock);

                await _stockRepo.SaveChangesAsync(cancellationToken);

                // 6. 🐰 Publish Event
                await _publishEndpoint.Publish(new StockAddedEvent
                {
                    ProductId = stock.ProductId,
                    QuantityAdded = request.Quantity,
                    NewStockLevel = stock.CurrentStock,
                    AddedAt = DateTime.UtcNow,
                    AddedBy = request.PerformedBy ?? "System"
                }, cancellationToken);

                // 7. Publish StockUpdated Event
                await _publishEndpoint.Publish(new StockUpdatedEvent
                {
                    ProductId = stock.ProductId,
                    CurrentStock = stock.CurrentStock,
                    MinStock = stock.MinStock,
                    MaxStock = stock.MaxStock,
                    UpdatedAt = DateTime.UtcNow,
                    IsLowStock = stock.IsLowStock
                }, cancellationToken);

                // 8. Return Response
                return Result.Success(new AddStockResponse(
                    stock.Id,
                    stock.CurrentStock,
                    stock.IsLowStock,
                    stock.IsOutOfStock
                ));
            }
        }
    }
}
