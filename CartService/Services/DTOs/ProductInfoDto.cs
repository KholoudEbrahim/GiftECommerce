namespace CartService.Services.DTOs
{
    public class ProductInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = default!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public bool IsActive { get; set; }
    }
}
