using CartService.Data;
using CartService.Models;
using CartService.Services;
using MediatR;

namespace CartService.Features.CartFeatures.Commands.AddCartItem
{
    public  record AddCartItemCommand(Guid? UserId, string? AnonymousId ,
        Guid ProductId,
        int Quantity) : IRequest<ResultDTO>
    {


        public class Handler : IRequestHandler<AddCartItemCommand, ResultDTO>
        {
            private readonly ICartRepository _cartRepository;
           private readonly IInventoryServiceClient _inventoryService;
            private readonly ILogger<Handler> _logger;

            public Handler(
                ICartRepository cartRepository,
                IInventoryServiceClient inventoryService,
                ILogger<Handler> logger)
            {
                _cartRepository = cartRepository;
                _inventoryService = inventoryService;
                _logger = logger;
            }

            public async Task<ResultDTO> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
            {
                // Validate product exists and is available
                var productInfo = await _inventoryService.GetProductInfoAsync(request.ProductId, cancellationToken);

                if (!productInfo.IsActive)
                    throw new InvalidOperationException($"Product {request.ProductId} is not active");

                // Validate stock availability
                var isAvailable = await _inventoryService.ValidateProductAvailabilityAsync(
                    request.ProductId, request.Quantity, cancellationToken);

                if (!isAvailable)
                    throw new InvalidOperationException($"Product {request.ProductId} is not available in requested quantity");

                // Get or create cart
                Cart cart = await GetOrCreateCartAsync(request, cancellationToken);

              
                var cartItem = CartItem.Create(
                    cart.Id,
                    request.ProductId,
                    productInfo.Name,
                    productInfo.Price,
                    productInfo.ImageUrl,
                    request.Quantity);

               
                cart.AddItem(cartItem);

                // Save changes
                await _cartRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Added product {ProductId} to cart {CartId} with quantity {Quantity}",
                    request.ProductId, cart.Id, request.Quantity);

                return new ResultDTO
                {
                    CartId = cart.Id,
                    ItemId = cartItem.Id,
                    TotalItems = cart.Items.Count,
                    SubTotal = cart.SubTotal,
                    Total = cart.Total
                };
            }

            private async Task<Cart> GetOrCreateCartAsync(AddCartItemCommand request, CancellationToken cancellationToken)
            {
                Cart? cart = null;

                if (request.UserId.HasValue)
                {
                    cart = await _cartRepository.GetActiveCartByUserIdAsync(request.UserId.Value, cancellationToken);
                }
                else if (!string.IsNullOrEmpty(request.AnonymousId))
                {
                    cart = await _cartRepository.GetActiveCartByAnonymousIdAsync(request.AnonymousId, cancellationToken);
                }

                if (cart == null)
                {
                    cart = CreateNewCart(request);
                    await _cartRepository.AddAsync(cart, cancellationToken);
                }

                return cart;
            }

            private Cart CreateNewCart(AddCartItemCommand request)
            {
                return request.UserId.HasValue
                    ? Cart.CreateForUser(request.UserId.Value)
                    : Cart.CreateForAnonymous(request.AnonymousId!);
            }
        }
    }
}







