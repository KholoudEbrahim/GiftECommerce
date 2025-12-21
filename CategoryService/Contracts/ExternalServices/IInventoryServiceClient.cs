using CategoryService.Contracts.ExternalServices.Dtos;

namespace CategoryService.Contracts.ExternalServices
{
    public interface IInventoryServiceClient
    {
        Task<StockInfoDto?> GetStockInfoAsync(int productId, CancellationToken cancellationToken = default);

    }
}
