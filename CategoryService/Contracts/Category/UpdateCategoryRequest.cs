namespace CategoryService.Contracts.Category;

public record UpdateCategoryRequest(int Id, string Name, string? ImageUrl, bool IsActive);

