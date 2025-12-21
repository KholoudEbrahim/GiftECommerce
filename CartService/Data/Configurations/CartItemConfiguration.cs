using CartService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CartService.Data.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(i => i.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.HasIndex(i => new { i.CartId, i.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_CartItems_CartId_ProductId");

            builder.Property(i => i.CreatedAt)
                .IsRequired();

            builder.Property(i => i.UpdatedAt)
                .IsRequired(false);
        }
    }
}
