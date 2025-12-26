namespace OrderService.Features.Endpoints.DTOs
{
    public record ReOrderRequest
    {
        public Guid? NewAddressId { get; init; }
        public List<ModifiedItemDto>? ModifiedItems { get; init; }
    }

}
