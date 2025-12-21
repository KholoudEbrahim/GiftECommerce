using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InventoryService.Contracts;
using InventoryService.DataBase;

namespace InventoryService.DataBase
{
    public class DbInitializer : IDbInitializer
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(InventoryDbContext context, ILogger<DbInitializer> logger)
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
                _logger.LogInformation("Applying pending migrations for InventoryDbContext...");
                await _context.Database.MigrateAsync();
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
}