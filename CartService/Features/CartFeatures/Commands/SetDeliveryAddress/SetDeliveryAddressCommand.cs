using CartService.Data;
using CartService.Models;
using CartService.Services;
using MediatR;
using static CartService.Features.CartFeatures.Commands.SetDeliveryAddress.SetDeliveryAddressCommand;

namespace CartService.Features.CartFeatures.Commands.SetDeliveryAddress
{
    public record SetDeliveryAddressCommand(Guid? UserId,
       string? AnonymousId,
        Guid AddressId) : IRequest<SelectDeliveryAddressDTO>
    {
 
    
        public class Handler : IRequestHandler<SetDeliveryAddressCommand, SelectDeliveryAddressDTO>
        {
            private readonly ICartRepository _cartRepository;
            private readonly IProfileServiceClient _profileService;
            private readonly ILogger<Handler> _logger;

            public Handler(
                ICartRepository cartRepository,
                IProfileServiceClient profileService,
                ILogger<Handler> logger)
            {
                _cartRepository = cartRepository;
                _profileService = profileService;
                _logger = logger;
            }

            public async Task<SelectDeliveryAddressDTO> Handle(SetDeliveryAddressCommand request, CancellationToken cancellationToken)
            {
        
                if (request.UserId.HasValue)
                {
                    var address = await _profileService.GetAddressAsync(
                        request.AddressId, request.UserId.Value, cancellationToken);

                    if (address == null)
                        throw new KeyNotFoundException($"Address {request.AddressId} not found for user");
                }

                var cart = await GetCartAsync(request, cancellationToken);

                if (cart == null)
                    throw new KeyNotFoundException("Active cart not found");

                cart.SetDeliveryAddress(request.AddressId);

                await _cartRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Set delivery address {AddressId} for cart {CartId}",
                    request.AddressId, cart.Id);

                return new SelectDeliveryAddressDTO
                {
                    CartId = cart.Id,
                    AddressId = request.AddressId,
                    DeliveryFee = cart.DeliveryFee,
                    Total = cart.Total
                };
            }

            private async Task<Cart?> GetCartAsync(SetDeliveryAddressCommand request, CancellationToken cancellationToken)
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
