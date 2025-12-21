namespace CategoryService.Contracts.Occasion
{
    public record CreateOccasionRequest(string Name, string? ImageUrl, bool IsActive);

}
