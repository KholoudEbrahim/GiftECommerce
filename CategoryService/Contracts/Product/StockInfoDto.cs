namespace CategoryService.Contracts.Product
{
    public record StockInfoDto(
    int CurrentStock,
    int MinStock,
    int MaxStock,
    bool IsLowStock,
    bool IsOutOfStock
);

}
