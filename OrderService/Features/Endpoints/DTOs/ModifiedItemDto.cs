namespace OrderService.Features.Endpoints.DTOs
{
    public record ModifiedItemDto
    {
        public int ProductId { get; init; }
        public int? NewQuantity { get; init; }
        public bool Remove { get; init; } = false;
    }
}
