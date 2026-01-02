using CartService.Data;
using CartService.Models;
using MediatR;

namespace CartService.Features.CartFeatures.Commands.SetGiftDetails
{
    public record SetGiftDetailsCommand(
        Guid? UserId,
        string? AnonymousId,
        string RecipientName,
        string RecipientPhone,
        string? GiftMessage = null,
        DateTime? DeliveryDate = null,
        bool GiftWrap = false) : IRequest<SetGiftDetailsResult>;
    public class Handler : IRequestHandler<SetGiftDetailsCommand, SetGiftDetailsResult>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(ICartRepository cartRepository, ILogger<Handler> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<SetGiftDetailsResult> Handle(SetGiftDetailsCommand request, CancellationToken cancellationToken)
        {
            var cart = await GetCartAsync(request, cancellationToken);

            if (cart == null)
                throw new KeyNotFoundException("Active cart not found");

            cart.MarkAsGift(
                request.RecipientName,
                request.RecipientPhone,
                request.GiftMessage,
                request.DeliveryDate,
                request.GiftWrap);

            await _cartRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Cart {CartId} marked as gift for recipient {RecipientName}",
                cart.Id, request.RecipientName);

            return new SetGiftDetailsResult
            {
                CartId = cart.Id,
                IsGift = cart.IsGift,
                GiftWrapFee = cart.GiftWrapFee,
                Total = cart.Total
            };
        }

        private async Task<Cart?> GetCartAsync(SetGiftDetailsCommand request, CancellationToken cancellationToken)
        {
            if (request.UserId.HasValue)
                return await _cartRepository.GetActiveCartByUserIdAsync(request.UserId.Value, cancellationToken);

            if (!string.IsNullOrEmpty(request.AnonymousId))
                return await _cartRepository.GetActiveCartByAnonymousIdAsync(request.AnonymousId, cancellationToken);

            return null;
        }
    }

}
