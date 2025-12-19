namespace CategoryService.Contracts.ExternalServices.Dtos
{
    public record StockInfoDto(
    int CurrentStock,
    int MinStock,
    int MaxStock,
    bool IsLowStock,
    bool IsOutOfStock
);
}
