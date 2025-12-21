namespace InventoryService.Contracts.Stock
{
    public record SetMinMaxStockRequest(
    int ProductId,
    int MinStock,
    int MaxStock
);

}
