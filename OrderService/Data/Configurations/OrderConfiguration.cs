using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Models;
using OrderService.Models.enums;

namespace OrderService.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .ValueGeneratedOnAdd();

            builder.Property(o => o.UserId)
                .IsRequired();

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v))
                .IsRequired();

            builder.Property(o => o.PaymentMethod)
                .HasConversion(
                    v => v.ToString(),
                    v => (PaymentMethod)Enum.Parse(typeof(PaymentMethod), v))
                .IsRequired();

            builder.Property(o => o.PaymentStatus)
                .HasConversion(
                    v => v.ToString(),
                    v => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), v))
                .IsRequired();

            builder.Property(o => o.SubTotal)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(o => o.DeliveryFee)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(o => o.Discount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(o => o.Tax)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(o => o.Total)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(o => o.DeliveryAddressId)
                .IsRequired();

            builder.Property(o => o.DeliveryAddressJson)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            builder.Property(o => o.Notes)
                .IsRequired(false)
                .HasMaxLength(1000);

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            builder.Property(o => o.UpdatedAt)
                .IsRequired(false);

            builder.Property(o => o.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(o => o.IsActive)
                .HasDefaultValue(true);
        }
    }
}
