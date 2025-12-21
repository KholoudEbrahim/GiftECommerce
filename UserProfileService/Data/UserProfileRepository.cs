using Microsoft.EntityFrameworkCore;
using UserProfileService.Models;

namespace UserProfileService.Data
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly UserProfileDbContext _context;

        public UserProfileRepository(UserProfileDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .Include(p => p.DeliveryAddresses)
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        }

        public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .Include(p => p.DeliveryAddresses)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
        {
            await _context.UserProfiles.AddAsync(userProfile, cancellationToken);
        }

        public Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
        {
            _context.UserProfiles.Update(userProfile);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .AnyAsync(p => p.UserId == userId, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
