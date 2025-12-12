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

        public async Task<List<Occasion>> GetAllActiveAsync(CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(o => o.IsActive)
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        public async Task<List<Occasion>> GetAllAsync(bool? isActive = null)
        {
            var query = _dbSet.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(o => o.IsActive == isActive.Value);
            }

            return await query
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        public void Update(Occasion occasion)
        {
            _dbSet.Update(occasion);
        }

        public void Delete(Occasion occasion)
        {
            occasion.IsDeleted = true;
            _dbSet.Update(occasion);
        }

        public void HardDelete(Occasion occasion)
        {
            _dbSet.Remove(occasion);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync(bool? isActive = null)
        {
            var query = _dbSet.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(o => o.IsActive == isActive.Value);
            }

            return await query.CountAsync();
        }

        public async Task<List<Occasion>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync();
            }

            return await _dbSet
                .Where(o => o.Name.Contains(searchTerm))
                .OrderBy(o => o.Name)
                .ToListAsync();
        }
    }
}
