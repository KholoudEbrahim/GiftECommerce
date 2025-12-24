using CartService.Data;
using CartService.Models;
using MediatR;
using static CartService.Features.CartFeatures.Commands.UpdateItemQuantity.UpdateCartItemQuantityCommand;

namespace CartService.Features.CartFeatures.Commands.UpdateItemQuantity
{
    public record UpdateCartItemQuantityCommand(Guid? UserId,
       string? AnonymousId ,
        int ProductId,
        int Quantity) : IRequest<CartItemQuantityDTO>
    {

        public class Handler : IRequestHandler<UpdateCartItemQuantityCommand, CartItemQuantityDTO>
        {
            private readonly ICartRepository _cartRepository;
            private readonly ILogger<Handler> _logger;

            public Handler(ICartRepository cartRepository, ILogger<Handler> logger)
            {
                _cartRepository = cartRepository;
                _logger = logger;
            }

            public async Task<CartItemQuantityDTO> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
            {
                var cart = await GetCartAsync(request, cancellationToken);

                if (cart == null)
                    throw new KeyNotFoundException("Active cart not found");

                bool itemRemoved = request.Quantity == 0;

                cart.UpdateItemQuantity(request.ProductId, request.Quantity);

                await _cartRepository.SaveChangesAsync(cancellationToken);

                var updatedItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

                _logger.LogInformation(
                    "Updated quantity for product {ProductId} in cart {CartId} to {Quantity}",
                    request.ProductId, cart.Id, request.Quantity);

                return new CartItemQuantityDTO
                {
                    CartId = cart.Id,
                    ItemId = updatedItem?.Id ?? 0,
                    Quantity = request.Quantity,
                    SubTotal = cart.SubTotal,
                    Total = cart.Total,
                    ItemRemoved = itemRemoved
                };
            }

            private async Task<Cart?> GetCartAsync(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
            {
                if (request.UserId.HasValue)
                {
                    return await _cartRepository.GetActiveCartByUserIdAsync(
                        request.UserId.Value, cancellationToken);
                }

                if (!string.IsNullOrEmpty(request.AnonymousId))
                {
                    return await _cartRepository.GetActiveCartByAnonymousIdAsync(
                        request.AnonymousId, cancellationToken);
                }

                return null;
            }
        }
    }
}