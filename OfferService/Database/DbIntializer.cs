using Microsoft.EntityFrameworkCore;
using OfferService.Contracts;
using OfferService.Database;
using OfferService.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OfferService.DataBase;

public class DbInitializer : IDbIntializer
{
    private readonly OfferDbContext _context;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(OfferDbContext context, ILogger<DbInitializer> logger)
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
            _logger.LogInformation("Applying pending migrations for OfferDbContext...");
            await _context.Database.MigrateAsync();
        }
    }

    public async Task SeedDataAsync()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());

            if (!await _context.Offers.AnyAsync())
            {
                _logger.LogInformation("Seeding Offers...");

                // Ensure the path matches where you put the file
                var data = await File.ReadAllTextAsync("DataBase/DataSeed/RawData/Offers.json");

                var offers = JsonSerializer.Deserialize<List<Offer>>(data, options);

                if (offers != null && offers.Any())
                {
                    // Ensure Dates are UTC (JSON sometimes reads as Unspecified)
                    foreach (var offer in offers)
                    {
                        if (offer.StartDateUtc.Kind != DateTimeKind.Utc)
                            offer.StartDateUtc = DateTime.SpecifyKind(offer.StartDateUtc, DateTimeKind.Utc);

                        if (offer.EndDateUtc.Kind != DateTimeKind.Utc)
                            offer.EndDateUtc = DateTime.SpecifyKind(offer.EndDateUtc, DateTimeKind.Utc);
                    }

                    await _context.Offers.AddRangeAsync(offers);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✅ Successfully seeded {offers.Count} offers.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ An error occurred while seeding offers.");
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