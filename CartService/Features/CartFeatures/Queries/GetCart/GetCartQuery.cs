using CartService.Data;
using CartService.Models;
using CartService.Models.enums;
using CartService.Services;
using CartService.Services.DTOs;
using MediatR;

namespace CartService.Features.CartFeatures.Queries.GetCart
{
    public record GetCartQuery(Guid? UserId,
       string? AnonymousId,
         bool IncludeAddressDetails) : IRequest<CartDetailsDto>
    {
   

  
        public class Handler : IRequestHandler<GetCartQuery, CartDetailsDto>
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

            public async Task<CartDetailsDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
            {
                Models.Cart? cart = await GetCartAsync(request, cancellationToken);

                if (cart == null)
                {
                    // Return empty cart result
                    return new CartDetailsDto
                    {
                        Items = new List<CartItemDto>(),
                        Status = Models.enums.CartStatus.Active
                    };
                }

                AddressDto? address = null;
                if (request.IncludeAddressDetails &&
                    cart.DeliveryAddressId.HasValue &&
                    cart.UserId.HasValue)
                {
                    address = await _profileService.GetAddressAsync(
                        cart.DeliveryAddressId.Value,
                        cart.UserId.Value,
                        cancellationToken);
                }

                return MapToResult(cart, address);
            }

            private async Task<Models.Cart?> GetCartAsync(GetCartQuery request, CancellationToken cancellationToken)
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

            private static CartDetailsDto MapToResult(Models.Cart cart, AddressDto? address)
            {
                return new CartDetailsDto
                {
                    CartId = cart.Id,
                    UserId = cart.UserId,
                    AnonymousId = cart.AnonymousId,
                    Status = cart.Status,
                    Items = cart.Items.Select(MapToItemDto).ToList(),
                    SubTotal = cart.SubTotal,
                    DeliveryFee = cart.DeliveryFee,
                    Total = cart.Total,
                    DeliveryAddressId = cart.DeliveryAddressId,
                    DeliveryAddress = address,                  
                    CreatedAt = cart.CreatedAt,
                    UpdatedAt = cart.UpdatedAt
                };
            }

            private static CartItemDto MapToItemDto(CartItem item)
            {
                return new CartItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Name = item.Name,
                    UnitPrice = item.UnitPrice,
                    ImageUrl = item.ImageUrl,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                };
            }
        }
    }
}
