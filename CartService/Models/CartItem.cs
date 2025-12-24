namespace CartService.Models
{
    public class CartItem : BaseEntity
    {
        public int CartId { get; private set; }
        public int ProductId { get; private set; }
        public string ProductName { get; private set; } = default!;
        public decimal UnitPrice { get; private set; }
        public string? ImageUrl { get; private set; }
        public int Quantity { get; private set; }
        public string Name { get; private set; } = default!;

        public decimal TotalPrice => UnitPrice * Quantity;

        public DateTime AddedAt { get; private set; }

        public Cart Cart { get; private set; } = default!;

        private CartItem() { }

        public static CartItem Create(
             int cartId,
               int productId,
             string name,
             decimal unitPrice,
             string imageUrl,
             int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

            if (unitPrice <= 0)
                throw new ArgumentException("Unit price must be greater than 0", nameof(unitPrice));

            return new CartItem
            {
             
                CartId = cartId,
                ProductId = productId,
                Name = name ?? throw new ArgumentNullException(nameof(name)),
                UnitPrice = unitPrice,
                ImageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl)),
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow,
                AddedAt = DateTime.UtcNow
            };
        }

        public void UpdateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

            Quantity = quantity;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
