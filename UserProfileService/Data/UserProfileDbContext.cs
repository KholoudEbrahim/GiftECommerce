using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UserProfileService.Models;

namespace UserProfileService.Data
{
    public class UserProfileDbContext : DbContext
    {
        public UserProfileDbContext()
        {
        }
        public UserProfileDbContext(DbContextOptions<UserProfileDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<DeliveryAddress> DeliveryAddresses { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserProfileDbContext).Assembly);

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.UserId)
                      .IsUnique();
                entity.HasQueryFilter(x => !x.IsDeleted);

                entity.HasMany(p => p.DeliveryAddresses)
                    .WithOne(a => a.UserProfile)
                   .HasForeignKey(a => a.UserProfileId)
                   .IsRequired();   
            });


            modelBuilder.Entity<DeliveryAddress>(entity =>
            {
                entity.HasKey(x => x.Id);


                entity.HasQueryFilter(x => !x.IsDeleted);
            });

            base.OnModelCreating(modelBuilder);
        }


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
