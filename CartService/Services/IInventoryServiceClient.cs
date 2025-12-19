using CartService.Services.DTOs;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CartService.Services
{
    public interface IInventoryServiceClient
    {
        Task<ProductInfoDto> GetProductInfoAsync(Guid productId, CancellationToken cancellationToken);

        Task<bool> ValidateProductAvailabilityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    }


}
