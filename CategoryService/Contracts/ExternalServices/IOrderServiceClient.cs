namespace CategoryService.Contracts.ExternalServices
{
    public interface IOrderServiceClient
    {
        Task<bool> IsProductInActiveOrdersAsync(int productId, CancellationToken cancellationToken = default);

        Task<int> GetProductTotalSalesAsync(int productId, CancellationToken cancellationToken = default);

    }
}
