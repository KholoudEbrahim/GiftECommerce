using MassTransit;

namespace CartService.Events.Consumer
{
    public class CartCheckedOutConsumer : IConsumer<CartCheckedOutEvent>
    {
        private readonly ILogger<CartCheckedOutConsumer> _logger;

        public CartCheckedOutConsumer(ILogger<CartCheckedOutConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CartCheckedOutEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Cart {CartId} checked out, order created: {OrderNumber}",
                message.CartId, message.OrderNumber);

        }
    }

}
