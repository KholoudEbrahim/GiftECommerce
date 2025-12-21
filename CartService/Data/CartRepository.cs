using CartService.Models;
using Microsoft.EntityFrameworkCore;

namespace CartService.Data
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _context;

        public CartRepository(CartDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetActiveCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == Models.enums.CartStatus.Active,
                    cancellationToken);

        }

        public async Task<Cart?> GetActiveCartByAnonymousIdAsync(string anonymousId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.AnonymousId == anonymousId && c.Status == Models.enums.CartStatus.Active,
                    cancellationToken);
        }

        public async Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .FindAsync(new object[] { cartId }, cancellationToken);
        }

        public async Task<Cart?> GetCartWithItemsAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
        }

        public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            await _context.Carts.AddAsync(cart, cancellationToken);
        }

        public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            _context.Carts.Update(cart);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .AnyAsync(c => c.Id == cartId, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}






