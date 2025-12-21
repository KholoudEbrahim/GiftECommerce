using Events.ProductEvents;
using InventoryService.Contracts;
using InventoryService.Models;
using InventoryService.Models.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Consumers
{
    public class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
    {
        private readonly IGenericRepository<Stock, int> _stockRepo;
        private readonly IGenericRepository<StockTransaction, int> _transactionRepo;
        private readonly ILogger<ProductCreatedConsumer> _logger;

        public ProductCreatedConsumer(
            IGenericRepository<Stock, int> stockRepo,
            IGenericRepository<StockTransaction, int> transactionRepo,
            ILogger<ProductCreatedConsumer> logger)
        {
            _stockRepo = stockRepo;
            _transactionRepo = transactionRepo;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Received ProductCreatedEvent for ProductId: {ProductId}, Name: {Name}",
                message.ProductId,
                message.Name);

            try
            {
                // Check if stock record already exists
                var existingStock = await _stockRepo
                    .GetAll(s => s.ProductId == message.ProductId)
                    .FirstOrDefaultAsync();

                if (existingStock != null)
                {
                    _logger.LogWarning(
                        "Stock record already exists for ProductId: {ProductId}",
                        message.ProductId);
                    return;
                }

                // Create new stock record with default values
                var stock = new Stock
                {
                    ProductId = message.ProductId,
                    ProductName = message.Name,
                    CurrentStock = 0,
                    MinStock = 10,      // Default min stock
                    MaxStock = 100,     // Default max stock
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _stockRepo.AddAsync(stock);
                await _stockRepo.SaveChangesAsync();

                // Create initial transaction
                var transaction = new StockTransaction
                {
                    StockId = stock.Id,
                    Type = StockTransactionType.InitialStock,
                    Quantity = 0,
                    StockBefore = 0,
                    StockAfter = 0,
                    Notes = $"Initial stock record created for product: {message.Name}",
                    PerformedBy = "System",
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _transactionRepo.AddAsync(transaction);
                await _transactionRepo.SaveChangesAsync();

                _logger.LogInformation(
                    "Stock record created successfully for ProductId: {ProductId}",
                    message.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating stock record for ProductId: {ProductId}",
                    message.ProductId);
                throw;
            }
        }
    }
}
