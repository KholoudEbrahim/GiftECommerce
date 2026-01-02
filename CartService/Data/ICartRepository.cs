using CartService.Models;

namespace CartService.Data
{
    public interface ICartRepository
    {
        Task<Cart?> GetActiveCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Cart?> GetActiveCartByAnonymousIdAsync(string anonymousId, CancellationToken cancellationToken = default);
        Task<Cart?> GetByIdAsync(int cartId, CancellationToken cancellationToken = default);
        Task<Cart?> GetCartWithItemsAsync(int cartId, CancellationToken cancellationToken = default);
        Task AddAsync(Cart cart, CancellationToken cancellationToken = default);
        Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int cartId, CancellationToken cancellationToken = default);
        Task<List<Cart>> GetCartsWithProductAsync(int productId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
