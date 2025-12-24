using CartService.Services.DTOs;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CartService.Services
{
    public interface IInventoryServiceClient
    {
        Task<ProductInfoDto> GetProductInfoAsync(int productId, CancellationToken cancellationToken);

        Task<bool> ValidateProductAvailabilityAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    }


}
