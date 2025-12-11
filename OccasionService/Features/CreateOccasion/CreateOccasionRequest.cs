namespace OccasionService.Features.CreateOccasion
{
    public record CreateOccasionRequest
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.Now;
        public string? ImageUrl { get; init; }
    }
}
