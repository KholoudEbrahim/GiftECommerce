namespace OrderService.Models
{
    public class Rating : BaseEntity
    {
        public Guid UserId { get; private set; }
        public int ProductId { get; private set; }
        public int OrderItemId { get; private set; }
        public int Score { get; private set; } 
        public string? Comment { get; private set; }
        public DateTime RatedAt { get; private set; }

        // Navigation
        public OrderItem OrderItem { get; private set; } = default!;

        private Rating() { }

        public static Rating Create(
            Guid userId,
            int productId,
            int orderItemId,
            int score,
            string? comment = null)
        {
            if (score < 1 || score > 5)
                throw new ArgumentException("Rating score must be between 1 and 5", nameof(score));

            if (score <= 3 && string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Comment is required for ratings 3 or below", nameof(comment));

            return new Rating
            {
                UserId = userId,
                ProductId = productId,
                OrderItemId = orderItemId,
                Score = score,
                Comment = comment,
                RatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };


        }
    }
}
