namespace InventoryService.Contracts.Stock
{
    public record AddStockRequest(
    int ProductId,
    int Quantity,
    string? Notes,
    string? PerformedBy
);

    public record AddStockResponse(
    int StockId,
    int CurrentStock,
    bool IsLowStock,
    bool IsOutOfStock
);


}
