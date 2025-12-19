namespace InventoryService.Contracts.Stock
{
    public record GetStockInfoResponse(
    int ProductId,
    string ProductName,
    int CurrentStock,
    int MinStock,
    int MaxStock,
    bool IsLowStock,
    bool IsOutOfStock,
    DateTime LastUpdated
);
}
