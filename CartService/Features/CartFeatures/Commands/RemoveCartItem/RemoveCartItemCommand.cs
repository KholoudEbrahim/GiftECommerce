using CartService.Data;
using CartService.Models;
using MediatR;
using static CartService.Features.CartFeatures.Commands.RemoveCartItem.RemoveCartItemCommand;

namespace CartService.Features.CartFeatures.Commands.RemoveCartItem
{
    public record RemoveCartItemCommand(
       Guid? UserId,
        string? AnonymousId,
        int ProductId) : IRequest<RemoveCartItemDTO>
    {



        public class Handler : IRequestHandler<RemoveCartItemCommand, RemoveCartItemDTO>
        {
            private readonly ICartRepository _cartRepository;
            private readonly ILogger<Handler> _logger;

            public Handler(ICartRepository cartRepository, ILogger<Handler> logger)
            {
                _cartRepository = cartRepository;
                _logger = logger;
            }

            public async Task<RemoveCartItemDTO> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
            {
                var cart = await GetCartAsync(request, cancellationToken);

                if (cart == null)
                    throw new KeyNotFoundException("Active cart not found");

                cart.RemoveItem(request.ProductId);

                await _cartRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Removed product {ProductId} from cart {CartId}",
                    request.ProductId, cart.Id);

                return new RemoveCartItemDTO
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    SubTotal = cart.SubTotal,
                    Total = cart.Total
                };
            }

            private async Task<Cart?> GetCartAsync(RemoveCartItemCommand request, CancellationToken cancellationToken)
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
