using Microsoft.EntityFrameworkCore;
using OccasionService.Models;
using System;

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
                entity.Property(e => e.CreatedAt).IsRequired();
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
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Wedding",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Occasion
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Birthday",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Occasion
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Anniversary",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var modifiedEntities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified)
                .Select(e => e.Entity as Shared.BaseEntity)
                .Where(e => e != null);

            foreach (var entity in modifiedEntities)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }



    }

   
}
