using System.Text.Json;
using CategoryService.Models;
using Microsoft.EntityFrameworkCore;
namespace CategoryService.DataBase;

using System.Text.Json.Serialization;
using CategoryService.Contracts;

public class DbInitializer : IDbIntializer
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(CatalogDbContext context, ILogger<DbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        await WaitForDatabaseAsync();

        var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            _logger.LogInformation("Applying pending migrations for CatalogDbContext...");
            await _context.Database.MigrateAsync();
        }
    }

    public async Task SeedDataAsync()
    {
        // for smooth enum insertions
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());

        // 1. CATEGORIES (Independent)
        if (!await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Seeding Categories...");
            var data = await File.ReadAllTextAsync(@"DataBase/DataSeed/RawData/Categories.json");
            var categories = JsonSerializer.Deserialize<List<Category>>(data , options);

            if (categories != null && categories.Any())
            {
                _context.Categories.AddRange(categories);
                await _context.SaveChangesAsync();
            }
        }

        // 2. OCCASIONS (Independent)
        if (!await _context.Occasions.AnyAsync())
        {
            _logger.LogInformation("Seeding Occasions...");
            var data = await File.ReadAllTextAsync(@"DataBase/DataSeed/RawData/Occasions.json");
            var occasions = JsonSerializer.Deserialize<List<Occasion>>(data , options);

            if (occasions != null && occasions.Any())
            {
                _context.Occasions.AddRange(occasions);
                await _context.SaveChangesAsync();
            }
        }

        // 3. PRODUCTS (Depends on Categories)
        if (!await _context.Products.AnyAsync())
        {
            _logger.LogInformation("Seeding Products...");
            var data = await File.ReadAllTextAsync(@"DataBase/DataSeed/RawData/Products.json");
            var products = JsonSerializer.Deserialize<List<Product>>(data , options);

            if (products != null && products.Any())
            {
                // Ensure IDs match your Categories.json (e.g., Flowers = 1)
                _context.Products.AddRange(products);
                await _context.SaveChangesAsync();
            }
        }

        // 4. PRODUCT-OCCASIONS (The Junction Table)
        if (!await _context.Set<ProductOccasion>().AnyAsync())
        {
            _logger.LogInformation("Seeding Product-Occasion Links...");
            var data = await File.ReadAllTextAsync(@"DataBase/DataSeed/RawData/ProductOccasions.json");

            var productOccasions = JsonSerializer.Deserialize<List<ProductOccasion>>(data , options);

            if (productOccasions != null && productOccasions.Any())
            {
                _context.Set<ProductOccasion>().AddRange(productOccasions);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task WaitForDatabaseAsync()
    {
        const int MaxRetries = 10;
        const int DelaySeconds = 5;
        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("✅ Database ensured!");
                if (await _context.Database.CanConnectAsync())
                {
                    _logger.LogInformation("✅ Database is connectable!");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Database connection attempt {i + 1} failed: {ex.Message}. Retrying...");
            }
            await Task.Delay(TimeSpan.FromSeconds(DelaySeconds));
        }
    }
}