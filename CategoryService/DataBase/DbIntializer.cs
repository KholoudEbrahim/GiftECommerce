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
            try
            {
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                // Check if it's a SQL exception with error 2714 (object already exists) or if it's wrapped
                Microsoft.Data.SqlClient.SqlException? sqlEx = ex as Microsoft.Data.SqlClient.SqlException 
                    ?? ex.InnerException as Microsoft.Data.SqlClient.SqlException;
                
                if (sqlEx != null && sqlEx.Number == 2714) // Object already exists
                {
                    _logger.LogWarning($"Migration conflict detected: {sqlEx.Message}. Tables may already exist. Marking migration as applied...");
                    // If migration fails due to existing objects, try to mark it as applied
                    try
                    {
                        // Ensure migration history table exists
                        await _context.Database.ExecuteSqlRawAsync(
                            "IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = '__EFMigrationsHistory') " +
                            "CREATE TABLE [__EFMigrationsHistory] ([MigrationId] nvarchar(150) NOT NULL, [ProductVersion] nvarchar(32) NOT NULL, CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId]))");
                        
                        var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                        var allMigrations = await _context.Database.GetMigrationsAsync();
                        foreach (var migration in allMigrations.Except(appliedMigrations))
                        {
                            try
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    $"IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '{migration}') " +
                                    $"INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('{migration}', '8.0.0')");
                                _logger.LogInformation($"Marked migration '{migration}' as applied.");
                            }
                            catch (Exception e)
                            {
                                _logger.LogWarning($"Could not mark migration '{migration}' as applied: {e.Message}");
                            }
                        }
                        _logger.LogInformation("✅ Migration conflict resolved. Service can continue.");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Failed to handle migration conflict: {e.Message}");
                        throw; // Re-throw if we can't handle it
                    }
                }
                else
                {
                    // Not a "table already exists" error, re-throw
                    _logger.LogError($"Migration failed with unexpected error: {ex.Message}");
                    throw;
                }
            }
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
                // Don't use EnsureCreatedAsync() - we use migrations instead
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
        throw new InvalidOperationException("Failed to connect to database after multiple retries.");
    }
}