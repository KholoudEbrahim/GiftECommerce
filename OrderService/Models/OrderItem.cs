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

        public int? Rating { get; private set; }
        public string? RatingComment { get; private set; }
        public DateTime? RatedAt { get; private set; }

  
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

        public void AddRating(int rating, string? comment)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

            if (rating <= 3 && string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Comment is required for ratings 3 or below", nameof(comment));

            Rating = rating;
            RatingComment = comment;
            RatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
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
