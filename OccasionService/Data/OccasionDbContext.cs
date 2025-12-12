using Microsoft.EntityFrameworkCore;
using OccasionService.Models;

namespace OccasionService.Data
{
    public class OccasionDbContext : DbContext
    {
        public OccasionDbContext(DbContextOptions<OccasionDbContext> options)
        : base(options)
        { }

        public DbSet<Occasion> Occasions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Occasion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAtUtc).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            SeedData(modelBuilder);
        }


        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Occasion>().HasData(
                new Occasion
                {
                    Id = 1,
                    Name = "Wedding",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Occasion
                {
                    Id = 2,
                    Name = "Birthday",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Occasion
                {
                    Id = 3,
                    Name = "Anniversary",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                }
            );
        }


        //public override Task<TKey> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    var modifiedEntities = ChangeTracker.Entries()
        //        .Where(e => e.State == EntityState.Modified)
        //        .Select(e => e.Entity as Shared.BaseEntity<)
        //        .Where(e => e != null);

        //    foreach (var entity in modifiedEntities)
        //    {
        //        entity.UpdatedAt = DateTime.UtcNow;
        //    }

        //    return base.SaveChangesAsync(cancellationToken);
        //}



    }

   
}
