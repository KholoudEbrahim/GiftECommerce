using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Models;

namespace OrderService.Data.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Id)
                .ValueGeneratedOnAdd();

            builder.Property(i => i.ProductId)
                .IsRequired();

            builder.Property(i => i.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Property(i => i.ImageUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(i => i.Discount)
                  .HasPrecision(18, 2)
                   .HasDefaultValue(null);

            // Indexes
            builder.HasIndex(i => new { i.OrderId, i.ProductId })
                .HasDatabaseName("IX_OrderItems_OrderId_ProductId");

            builder.HasIndex(i => i.ProductId)
                .HasDatabaseName("IX_OrderItems_ProductId");

            // Query filter
            builder.HasQueryFilter(i => !i.IsDeleted && i.IsActive);
        }
    }
}
