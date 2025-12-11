namespace OccasionService.Features.UpdateOccasion
{
    public record UpdateOccasionRequest
    {
        public string Name { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public string? ImageUrl { get; init; }
    }
}
