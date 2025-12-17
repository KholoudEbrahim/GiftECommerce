namespace InventoryService.Contracts
{
    public interface IDbInitializer
    {
        Task MigrateAsync();

    }
}
