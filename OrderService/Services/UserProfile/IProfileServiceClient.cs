using OrderService.Services.DTOs;

namespace OrderService.Services.UserProfile
{
    public interface IProfileServiceClient
    {
        Task<UserProfileDto?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<AddressDto>?> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<AddressDto?> GetAddressByIdAsync(Guid addressId, CancellationToken cancellationToken = default);
    }
}
