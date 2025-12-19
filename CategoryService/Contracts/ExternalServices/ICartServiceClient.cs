namespace CategoryService.Contracts.ExternalServices
{
    public interface ICartServiceClient
    {
        Task<bool> IsProductInActiveCartsAsync(int productId, CancellationToken cancellationToken = default);

        Task<int> GetReservedQuantityAsync(int productId, CancellationToken cancellationToken = default);

    }
}
