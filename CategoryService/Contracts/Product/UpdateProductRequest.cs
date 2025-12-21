namespace CategoryService.Contracts.Product
{
    public record UpdateProductRequest(
    int Id,
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
