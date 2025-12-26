namespace OrderService.Services.DTOs
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = default!;
        public decimal UnitPrice { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; } =0;
        public decimal TotalPrice => UnitPrice * Quantity - Discount;
             public CartItemDto() { }
        
        public CartItemDto(int productId, string name, decimal unitPrice, int quantity, string? imageUrl, decimal discount = 0)
        {
            ProductId = productId;
            Name = name;
            UnitPrice = unitPrice;
            Quantity = quantity;
            ImageUrl = imageUrl;
            Discount = discount;
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    public class ApiErrorResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public string? Details { get; set; }
        public string? CorrelationId { get; set; }
    }
}
