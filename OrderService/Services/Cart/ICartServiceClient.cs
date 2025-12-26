using OrderService.Services.DTOs;

namespace OrderService.Services.Cart
{
    public interface ICartServiceClient
    {
        Task<CartDto?> GetCartByIdAsync(Guid cartId, CancellationToken cancellationToken = default);
        Task<CartDto?> GetActiveCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default);
        Task<bool> ValidateCartItemsAsync(Guid cartId, CancellationToken cancellationToken = default);
    }
}
