using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Models;
using OrderService.Models.enums;

namespace OrderService.Data.Configurations
{
    public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
    {
        public void Configure(EntityTypeBuilder<Delivery> builder)
        {
            builder.ToTable("Deliveries");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .ValueGeneratedOnAdd();

            builder.Property(d => d.OrderId)
                .IsRequired();

            builder.Property(d => d.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (DeliveryStatus)Enum.Parse(typeof(DeliveryStatus), v))
                .IsRequired();

            builder.Property(d => d.EstimatedDeliveryTime)
                .IsRequired(false);

            builder.Property(d => d.ActualDeliveryTime)
                .IsRequired(false);

            builder.Property(d => d.DeliveryHeroId)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(d => d.DeliveryHeroName)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(d => d.DeliveryHeroPhone)
                .IsRequired(false)
                .HasMaxLength(20);

            builder.Property(d => d.CurrentLatitude)
                .HasPrecision(9, 6)
                .IsRequired(false);

            builder.Property(d => d.CurrentLongitude)
                .HasPrecision(9, 6)
                .IsRequired(false);

            builder.Property(d => d.TrackingUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(d => d.Notes)
                .IsRequired(false)
                .HasMaxLength(1000);

            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.Property(d => d.UpdatedAt)
                .IsRequired(false);

            builder.Property(d => d.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(d => d.IsActive)
                .HasDefaultValue(true);
        }
    }
}
