namespace CategoryService.Contracts;

public interface IDbIntializer
{
    Task MigrateAsync();
    Task SeedDataAsync();

}
