namespace CategoryService.Contracts.Product
{
    public record GetProductsResponse(
    int Id,
    string Name,
    decimal Price,
    decimal? Discount,
    string ImageUrl,
    string Status,
    string CategoryName,
    List<string> OccasionNames
);

}
