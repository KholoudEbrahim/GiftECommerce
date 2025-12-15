namespace CategoryService.Contracts.Product
{
    public record SearchProductsRequest(
    string? SearchTerm,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? CategoryId,
    int? OccasionId,
    List<string>? Tags,
    int PageNumber = 1,
    int PageSize = 20
);

}
