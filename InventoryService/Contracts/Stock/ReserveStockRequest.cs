namespace InventoryService.Contracts.Stock
{
    public record ReserveStockRequest(
    int ProductId,
    int Quantity,
    string OrderId
);
}
