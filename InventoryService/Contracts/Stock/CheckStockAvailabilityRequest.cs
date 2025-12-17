namespace InventoryService.Contracts.Stock
{
    public record CheckStockAvailabilityRequest(
    int ProductId,
    int RequestedQuantity
);

    public record CheckStockAvailabilityResponse(
    bool IsAvailable,
    int CurrentStock,
    int RequestedQuantity,
    string? Message
);
}
