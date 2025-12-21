namespace CategoryService.Contracts.Occasion
{
    public record UpdateOccasionRequest(int Id, string Name, string? ImageUrl, bool IsActive);

}
