using Microsoft.EntityFrameworkCore;
using OccasionService.Models;

namespace OccasionService.Data
{
    public class OccasionRepository
    {
        protected readonly OccasionDbContext _context;
        protected readonly DbSet<Occasion> _dbSet;

        public OccasionRepository(OccasionDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Occasion>();
        }

        public async Task<Occasion> AddAsync(Occasion occasion)
        {
            var entityEntry = await _dbSet.AddAsync(occasion);
            await _context.SaveChangesAsync();
            return entityEntry.Entity;
        }

        public async Task<Occasion?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _dbSet.AnyAsync(o => o.Name == name);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
