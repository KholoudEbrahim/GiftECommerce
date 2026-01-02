using OrderService.Services.DTOs;

namespace OrderService.Models
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; private set; }
        public int ProductId { get; private set; }
        public string ProductName { get; private set; } = default!;
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }
        public string? ImageUrl { get; private set; }
        public decimal Discount { get; private set; } = 0;
        public decimal TotalPrice => UnitPrice * Quantity - Discount;

        public int? GetLatestRating() =>
               Ratings?.OrderByDescending(r => r.RatedAt).FirstOrDefault()?.Score;

        public string? GetLatestRatingComment() =>
            Ratings?.OrderByDescending(r => r.RatedAt).FirstOrDefault()?.Comment;

        public DateTime? GetLatestRatedAt() =>
            Ratings?.OrderByDescending(r => r.RatedAt).FirstOrDefault()?.RatedAt;

        public Order Order { get; private set; } = default!;
        public ICollection<Rating>? Ratings { get; private set; }

        private OrderItem() { }

        public static OrderItem Create(
            int orderId,
            int productId,
            string productName,
            decimal unitPrice,
            int quantity,
            string? imageUrl,
            decimal discount = 0)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

            if (unitPrice <= 0)
                throw new ArgumentException("Unit price must be greater than 0", nameof(unitPrice));

            return new OrderItem
            {
                OrderId = orderId,
                ProductId = productId,
                ProductName = productName ?? throw new ArgumentNullException(nameof(productName)),
                UnitPrice = unitPrice,
                Quantity = quantity,
                ImageUrl = imageUrl,
                Discount = discount
            };
        }

        public void UpdateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

            Quantity = quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public static OrderItem FromCartItem(CartItemDto cartItem, int orderId)
        {
            return Create(
                orderId: orderId,
                productId: cartItem.ProductId,
                productName: cartItem.Name,
                unitPrice: cartItem.UnitPrice,
                quantity: cartItem.Quantity,
                imageUrl: cartItem.ImageUrl,
                discount: cartItem.Discount
            );
        }
    }
}