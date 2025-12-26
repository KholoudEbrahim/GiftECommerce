namespace OrderService.Services.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public Guid? UserId { get; set; }
        public string? AnonymousId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
}
