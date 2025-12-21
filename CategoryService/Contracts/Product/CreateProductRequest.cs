namespace CategoryService.Contracts.Product
{
    public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    decimal? Discount,
    int CategoryId,
    List<int> OccasionIds,
    List<string>? Tags,
    string? ImageUrl,
    bool IsActive
);
}
