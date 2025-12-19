using CartService.Models;

namespace CartService.Data
{
    public interface ICartRepository
    {
        Task<Cart?> GetActiveCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Cart?> GetActiveCartByAnonymousIdAsync(string anonymousId, CancellationToken cancellationToken = default);
        Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default);
        Task<Cart?> GetCartWithItemsAsync(Guid cartId, CancellationToken cancellationToken = default);
        Task AddAsync(Cart cart, CancellationToken cancellationToken = default);
        Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid cartId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
