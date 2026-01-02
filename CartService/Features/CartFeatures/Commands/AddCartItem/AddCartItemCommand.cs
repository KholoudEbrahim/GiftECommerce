using CartService.Data;
using CartService.Models;
using CartService.Services;
using MediatR;

namespace CartService.Features.CartFeatures.Commands.AddCartItem
{
    public record AddCartItemCommand(
          Guid? UserId,
          string? AnonymousId,
          int ProductId,
          int Quantity) : IRequest<ResultDTO>
    {
        public class Handler : IRequestHandler<AddCartItemCommand, ResultDTO>
        {
            private readonly ICartRepository _cartRepository;
            private readonly IInventoryServiceClient _inventoryService;
            private readonly ILogger<Handler> _logger;
            private readonly Shared.IUserContext _userContext;

            public Handler(
                ICartRepository cartRepository,
                IInventoryServiceClient inventoryService,
                ILogger<Handler> logger,
                Shared.IUserContext userContext)
            {
                _cartRepository = cartRepository;
                _inventoryService = inventoryService;
                _logger = logger;
                _userContext = userContext;
            }

            public async Task<ResultDTO> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    _logger.LogInformation(
                        "AddCartItemCommand started - UserId: {UserId}, AnonymousId: {AnonymousId}, ProductId: {ProductId}, Quantity: {Quantity}",
                        request.UserId, request.AnonymousId, request.ProductId, request.Quantity);

      
                    var effectiveUserId = request.UserId ?? _userContext.UserId;
                    var effectiveAnonymousId = request.AnonymousId ?? _userContext.AnonymousId;

                    _logger.LogInformation(
                        "Effective values - UserId: {UserId}, AnonymousId: {AnonymousId}",
                        effectiveUserId, effectiveAnonymousId);

   
                    if (!effectiveUserId.HasValue && string.IsNullOrEmpty(effectiveAnonymousId))
                    {
                        throw new InvalidOperationException("Neither UserId nor AnonymousId is available");
                    }


                    _logger.LogInformation("Getting product info for ProductId: {ProductId}", request.ProductId);
                    var productInfo = await _inventoryService.GetProductInfoAsync(request.ProductId, cancellationToken);

                    if (!productInfo.IsActive)
                    {
                        throw new InvalidOperationException($"Product {request.ProductId} is not active");
                    }


                    _logger.LogInformation("Checking availability for ProductId: {ProductId}, Quantity: {Quantity}",
                        request.ProductId, request.Quantity);
                    var isAvailable = await _inventoryService.ValidateProductAvailabilityAsync(
                        request.ProductId, request.Quantity, cancellationToken);

                    if (!isAvailable)
                    {
                        throw new InvalidOperationException($"Product {request.ProductId} is not available in requested quantity");
                    }

  
                    Cart cart;
                    if (effectiveUserId.HasValue)
                    {
                        _logger.LogInformation("Getting cart for UserId: {UserId}", effectiveUserId.Value);
                        cart = await _cartRepository.GetActiveCartByUserIdAsync(
                            effectiveUserId.Value, cancellationToken);
                    }
                    else
                    {
                        _logger.LogInformation("Getting cart for AnonymousId: {AnonymousId}", effectiveAnonymousId);
                        cart = await _cartRepository.GetActiveCartByAnonymousIdAsync(
                            effectiveAnonymousId!, cancellationToken);
                    }

                    if (cart == null)
                    {
                        _logger.LogInformation("Creating new cart");
                        cart = effectiveUserId.HasValue
                            ? Cart.CreateForUser(effectiveUserId.Value)
                            : Cart.CreateForAnonymous(effectiveAnonymousId!);

                        await _cartRepository.AddAsync(cart, cancellationToken);
                    }

                    _logger.LogInformation("Product info - Name: {ProductName}, Price: {Price}, Image: {ImageUrl}",
                          productInfo.Name, productInfo.Price, productInfo.ImageUrl);
                    var cartItem = CartItem.Create(
                              cart.Id,
                              request.ProductId,
                              productInfo.Name ?? $"Product {request.ProductId}", 
                              productInfo.Price,
                              productInfo.ImageUrl ?? "https://example.com/product.jpg",
                              request.Quantity);



                    cart.AddItem(cartItem);
                    await _cartRepository.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Successfully added product {ProductId} to cart {CartId}",
                        request.ProductId, cart.Id);

                    return new ResultDTO
                    {
                        CartId = cart.Id,
                        ItemId = cartItem.Id,
                        TotalItems = cart.Items.Count,
                        SubTotal = cart.SubTotal,
                        Total = cart.Total
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AddCartItemCommand");
                    throw;
                }
            }
        }
    }

}


