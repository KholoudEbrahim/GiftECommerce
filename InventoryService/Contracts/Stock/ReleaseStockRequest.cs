namespace InventoryService.Contracts.Stock
{
    public record ReleaseStockRequest(
    int ProductId,
    int Quantity,
    string OrderId
);
}
