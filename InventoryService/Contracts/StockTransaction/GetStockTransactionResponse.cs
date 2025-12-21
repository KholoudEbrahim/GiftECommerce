namespace InventoryService.Contracts.StockTransaction
{
    public record GetStockTransactionsResponse(
    int Id,
    string TransactionType,
    int Quantity,
    int StockBefore,
    int StockAfter,
    string? Reference,
    string? Notes,
    string? PerformedBy,
    DateTime CreatedAt
);
}
