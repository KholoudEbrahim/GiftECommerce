namespace OrderService.Features.Tracking.DTOs
{
    public record LocationDto
    {
        public decimal Latitude { get; init; }
        public decimal Longitude { get; init; }
    }
}
