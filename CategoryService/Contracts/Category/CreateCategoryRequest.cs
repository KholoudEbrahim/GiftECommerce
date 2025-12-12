namespace CategoryService.Contracts.Category;

public record CreateCategoryRequest(string Name, string? ImageUrl, bool IsActive);
