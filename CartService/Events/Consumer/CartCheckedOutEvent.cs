namespace CartService.Events.Consumer
{
    public record CartCheckedOutEvent
    {
        public int CartId { get; init; }
        public Guid? UserId { get; init; }
        public int OrderId { get; init; } 
        public string OrderNumber { get; init; } = default!;
        public decimal TotalAmount { get; init; }
        public DateTime CheckedOutAt { get; init; }
        public List<CartItemEvent> Items { get; init; } = new();
    }
}
