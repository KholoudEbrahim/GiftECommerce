namespace OccasionService.Features.GetAllOccasions
{
    public record OccasionDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public bool IsActive { get; init; }
        public string ImageUrl { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
