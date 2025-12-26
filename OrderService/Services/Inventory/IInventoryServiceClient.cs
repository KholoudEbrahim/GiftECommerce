using OrderService.Services.DTOs;

namespace OrderService.Services.Inventory
{
    public interface IInventoryServiceClient
    {
        Task<ProductDto?> GetProductInfoAsync(int productId, CancellationToken cancellationToken = default);
        Task<bool> ValidateProductAvailabilityAsync(int productId, int quantity, CancellationToken cancellationToken = default);
        Task<ProductAvailabilityResponse?> CheckProductAvailabilityAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    }
}
