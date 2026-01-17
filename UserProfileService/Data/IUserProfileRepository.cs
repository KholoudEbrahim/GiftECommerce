using UserProfileService.Models;

namespace UserProfileService.Data
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<DeliveryAddress?> GetAddressByIdAsync(Guid addressId, CancellationToken cancellationToken = default);
        Task AddAddressAsync(
          DeliveryAddress address,
          CancellationToken cancellationToken = default);

        Task UnsetPrimaryAddressesAsync(
            Guid userProfileId,
            CancellationToken cancellationToken = default);

        Task<bool> IsAddressOwnedByUserAsync(Guid addressId, Guid userId, CancellationToken cancellationToken = default);
    }
}

