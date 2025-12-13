namespace CategoryService.Contracts.Occasion
{
    public sealed record GetOccasionDetailsResponse(
    int Id,
    string Name,
    string ImageUrl,
    string Status,
    List<OccasionProductDto> Products
);


    public sealed record OccasionProductDto(
    int Id,
    string Name,
    decimal Price,
    decimal? Discount,
    string ImageUrl,
    string Status,
    string CategoryName
);

}
