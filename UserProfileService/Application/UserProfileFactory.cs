using Microsoft.EntityFrameworkCore;
using UserProfileService.Data;
using UserProfileService.Models;

namespace UserProfileService.Application
{
    public static class UserProfileFactory
    {
        public static async Task<UserProfile> GetOrCreateAsync(
            Guid userId,
            IUserProfileRepository repository,
            CancellationToken cancellationToken)
        {
            var profile = await repository.GetByUserIdAsync(userId, cancellationToken);
            if (profile != null)
                return profile;

            profile = new UserProfile(userId, "User", "User");

            try
            {
                await repository.AddAsync(profile, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);
                return profile;
            }
            catch (DbUpdateException)
            {
                var existing = await repository.GetByUserIdAsync(userId, cancellationToken);
                if (existing != null)
                    return existing;

                throw;
            }
        }
    }

}
