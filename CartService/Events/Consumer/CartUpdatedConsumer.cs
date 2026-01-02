using MassTransit;

namespace CartService.Events.Consumer
{
    public class CartUpdatedConsumer : IConsumer<CartUpdatedEvent>
    {
        private readonly ILogger<CartUpdatedConsumer> _logger;

        public CartUpdatedConsumer(ILogger<CartUpdatedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CartUpdatedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Cart {CartId} updated with {ItemCount} items, total: {TotalAmount}",
                message.CartId, message.ItemCount, message.TotalAmount);

        }
    }
}
