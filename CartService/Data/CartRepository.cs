using CartService.Models;
using CartService.Models.enums;
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
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active,
                    cancellationToken);
        }



        public async Task<Cart?> GetActiveCartByAnonymousIdAsync(string anonymousId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.AnonymousId == anonymousId && c.Status == Models.enums.CartStatus.Active,
                    cancellationToken);
        }

        public async Task<Cart?> GetByIdAsync(int cartId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .FindAsync(new object[] { cartId }, cancellationToken);
        }

        public async Task<Cart?> GetCartWithItemsAsync(int cartId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
        }

        public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            await _context.Carts.AddAsync(cart, cancellationToken);
        }

        public Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            _context.Entry(cart).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(int cartId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .AnyAsync(c => c.Id == cartId, cancellationToken);
        }
        public async Task<List<Cart>> GetCartsWithProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .Where(c => c.Status == CartStatus.Active &&
                           c.Items.Any(i => i.ProductId == productId))
                .ToListAsync(cancellationToken);
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}






