namespace CategoryService.Contracts.Category;

public sealed record GetCategoriesResponse(int Id, string Name, string ImageUrl, string Status);