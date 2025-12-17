using InventoryService.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared;
using System.Linq.Expressions;

namespace InventoryService.DataBase
{
    public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
    {
        protected readonly InventoryDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(InventoryDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public IQueryable<TEntity> GetAll(bool trackChanges = false) =>
            !trackChanges
                ? _dbSet.AsNoTracking()
                : _dbSet;

        public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> expression, bool trackChanges = false) =>
            !trackChanges
                ? _dbSet.AsNoTracking().Where(expression)
                : _dbSet.Where(expression);

        public Task<TEntity?> GetByIdAsync(TKey id, bool trackChanges = false)
            => !trackChanges
                ? _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id!.Equals(id))
                : _dbSet.FirstOrDefaultAsync(e => e.Id!.Equals(id));

        public async Task AddAsync(TEntity entity) =>
            await _dbSet.AddAsync(entity);

        public async Task AddRangeAsync(IEnumerable<TEntity> entities) =>
            await _dbSet.AddRangeAsync(entities);

        public void Update(TEntity entity) =>
            _dbSet.Update(entity);

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
            => await _dbSet.AsNoTracking().AnyAsync(expression, cancellationToken);

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            await _context.SaveChangesAsync(cancellationToken);

        public virtual async Task DeleteAsync(TKey id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                _dbSet.Update(entity);
            }
        }
    }
}
