using OrderService.Services.DTOs;

namespace OrderService.Services.Cart
{
    public interface ICartServiceClient
    {
        Task<CartDto?> GetActiveCartByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<bool> ValidateActiveCartAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<bool> ClearCartAsync(
            CancellationToken cancellationToken = default);
    }
}
