namespace CategoryService.Contracts.Product
{
    public record GetProductDetailsResponse(
     int Id,
     string Name,
     string Description,
     decimal Price,
     decimal? Discount,
     string ImageUrl,
     string Status,
     CategoryDto Category,
     List<OccasionDto> Occasions,
     List<string>? Tags,
     StockInfoDto? StockInfo,
     DateTime CreatedAt
 );
}
