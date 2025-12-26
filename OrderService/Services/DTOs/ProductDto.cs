namespace OrderService.Services.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
    }

    public class ProductAvailabilityResponse
    {
        public bool IsAvailable { get; set; }
        public int AvailableQuantity { get; set; }
        public string? Message { get; set; }
    }
}
