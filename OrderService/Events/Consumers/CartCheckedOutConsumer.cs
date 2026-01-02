using MassTransit;
using OrderService.Data;
using OrderService.Events.Publisher;
using OrderService.Models.enums;

namespace OrderService.Events.Consumers
{
    public class CartCheckedOutConsumer : IConsumer<CartCheckedOutEvent>
    {
        private readonly ILogger<CartCheckedOutConsumer> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IEventPublisher _eventPublisher; 

        public CartCheckedOutConsumer(
            ILogger<CartCheckedOutConsumer> logger,
            IOrderRepository orderRepository,
            IEventPublisher eventPublisher) 
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task Consume(ConsumeContext<CartCheckedOutEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Cart {CartId} checked out, order created: {OrderNumber}",
                message.CartId, message.OrderNumber);

            try
            {
                var order = await _orderRepository.GetByOrderNumberAsync(message.OrderNumber);
                if (order != null)
                {
                   
                    order.ConfirmOrder();
                    await _orderRepository.SaveChangesAsync();

 
                    await _eventPublisher.PublishOrderStatusUpdatedAsync(
                        order,
                        OrderStatus.Pending.ToString(),
                        context.CancellationToken);

                    _logger.LogInformation(
                        "Order {OrderNumber} confirmed after cart checkout",
                        message.OrderNumber);
                }
                else
                {
                    _logger.LogWarning(
                        "Order {OrderNumber} not found after cart checkout",
                        message.OrderNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing cart checkout for order {OrderNumber}",
                    message.OrderNumber);

                throw;
            }
        }
    }
}

