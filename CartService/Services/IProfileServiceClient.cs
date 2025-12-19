using CartService.Services.DTOs;

namespace CartService.Services
{
    public interface IProfileServiceClient
    {
        Task<AddressDto?> GetAddressAsync(Guid addressId, Guid userId, CancellationToken cancellationToken = default);
        Task<List<AddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
    }

}
