namespace CategoryService.Contracts.ExternalServices.Dtos
{
    public record OrderCheckResponse(
    bool IsInActiveOrders,
    int TotalOrders,
    int TotalQuantitySold
);
}
