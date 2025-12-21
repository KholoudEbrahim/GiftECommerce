using FluentValidation;
using InventoryService.Contracts;
using InventoryService.Models;
using InventoryService.shared.MarkerInterface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.ApiResultResponse;

namespace InventoryService.Features
{
    public static class SetMinMaxStock
    {
        public sealed record Command(
        int ProductId,
        int MinStock,
        int MaxStock
    ) : ICommand<Result<int>>;

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.ProductId)
                    .GreaterThan(0)
                    .WithMessage("Valid product ID is required");

                RuleFor(x => x.MinStock)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Minimum stock cannot be negative");

                RuleFor(x => x.MaxStock)
                    .GreaterThan(0)
                    .WithMessage("Maximum stock must be greater than 0");

                RuleFor(x => x)
                    .Must(x => x.MaxStock >= x.MinStock)
                    .WithMessage("Maximum stock must be greater than or equal to minimum stock");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<int>>
        {
            private readonly IGenericRepository<Stock, int> _stockRepo;
            private readonly IValidator<Command> _validator;

            public Handler(
                IGenericRepository<Stock, int> stockRepo,
                IValidator<Command> validator)
            {
                _stockRepo = stockRepo;
                _validator = validator;
            }

            public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
            {
                // 1. Validation
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure<int>(
                        new Error("SetMinMaxStock.Validation", validationResult.ToString()));

                // 2. Get or Create Stock Record
                var stock = await _stockRepo
                    .GetAll(s => s.ProductId == request.ProductId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (stock == null)
                {
                    // Create new stock record if doesn't exist
                    stock = new Stock
                    {
                        ProductId = request.ProductId,
                        ProductName = $"Product_{request.ProductId}", // Will be updated by event
                        CurrentStock = 0,
                        MinStock = request.MinStock,
                        MaxStock = request.MaxStock,
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    await _stockRepo.AddAsync(stock);
                }
                else
                {
                    // Update existing stock limits
                    stock.MinStock = request.MinStock;
                    stock.MaxStock = request.MaxStock;
                    stock.UpdatedAtUtc = DateTime.UtcNow;

                    _stockRepo.Update(stock);
                }

                await _stockRepo.SaveChangesAsync(cancellationToken);

                return Result.Success(stock.ProductId);
            }
        }



    }
}
