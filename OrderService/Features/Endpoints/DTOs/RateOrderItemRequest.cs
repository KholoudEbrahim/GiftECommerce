namespace OrderService.Features.Endpoints.DTOs
{

    public record RateOrderItemRequest
    {
        public required int Rating { get; init; }
        public string? Comment { get; init; }
    }
}
