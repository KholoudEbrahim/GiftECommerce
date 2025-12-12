namespace CategoryService.Contracts.Category;

public sealed record GetCategoryDetailsResponse(
      int Id,
      string Name,
      string ImageUrl,
      string Status,
      List<ProductDto> Products
  );

public sealed record ProductDto(
    int Id,
    string Name,
    decimal Price,
    decimal? Discount,
    string ImageUrl,
    string Status
);
