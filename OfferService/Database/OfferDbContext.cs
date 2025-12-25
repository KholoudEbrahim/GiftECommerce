using Microsoft.EntityFrameworkCore;
using OfferService.Models;

namespace OfferService.Database;

public class OfferDbContext : DbContext
{
    public OfferDbContext(DbContextOptions<OfferDbContext> options) : base(options) { }

    public DbSet<Offer> Offers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OfferDbContext).Assembly);
    }
}