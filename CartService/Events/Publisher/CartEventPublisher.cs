using CartService.Events.Consumer;
using CartService.Models;
using MassTransit;

namespace CartService.Events.Publisher
{
    public class CartEventPublisher : ICartEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CartEventPublisher> _logger;

        public CartEventPublisher(
            IPublishEndpoint publishEndpoint,
            ILogger<CartEventPublisher> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task PublishCartUpdatedAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            var @event = new CartUpdatedEvent
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                AnonymousId = cart.AnonymousId,
                TotalAmount = cart.Total,
                ItemCount = cart.Items.Count,
                UpdatedAt = DateTime.UtcNow,
                Items = cart.Items.Select(i => new CartItemEvent
                {
                    ProductId = i.ProductId,
                    ProductName = i.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await _publishEndpoint.Publish(@event, cancellationToken);
            _logger.LogInformation("Published CartUpdatedEvent for cart {CartId}", cart.Id);
        }

        public async Task PublishCartCheckedOutAsync(Cart cart, int orderId, string orderNumber, CancellationToken cancellationToken = default)
        {
            var @event = new CartCheckedOutEvent
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                OrderId = orderId, 
                OrderNumber = orderNumber, 
                TotalAmount = cart.Total,
                CheckedOutAt = DateTime.UtcNow,
                Items = cart.Items.Select(i => new CartItemEvent
                {
                    ProductId = i.ProductId,
                    ProductName = i.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await _publishEndpoint.Publish(@event, cancellationToken);
        }
    }
}

