using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Models;

namespace OrderService.Data.Configurations
{
    public class RatingConfiguration : IEntityTypeConfiguration<Rating>
    {
        public void Configure(EntityTypeBuilder<Rating> builder)
        {
            builder.ToTable("Ratings");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedOnAdd();

            builder.Property(r => r.UserId)
                .IsRequired();

            builder.Property(r => r.ProductId)
                .IsRequired();

            builder.Property(r => r.OrderItemId)
                .IsRequired();

            builder.Property(r => r.Score)
                .IsRequired();

            builder.Property(r => r.Comment)
                .IsRequired(false)
                .HasMaxLength(1000);

            builder.Property(r => r.RatedAt)
                .IsRequired();

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.Property(r => r.UpdatedAt)
                .IsRequired(false);

            builder.Property(r => r.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(r => r.IsActive)
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(r => new { r.UserId, r.OrderItemId })
                .IsUnique()
                .HasDatabaseName("IX_Ratings_UserId_OrderItemId");

            builder.HasIndex(r => r.ProductId)
                .HasDatabaseName("IX_Ratings_ProductId");

            builder.HasIndex(r => r.OrderItemId)
                .HasDatabaseName("IX_Ratings_OrderItemId");
        }
    }
}
