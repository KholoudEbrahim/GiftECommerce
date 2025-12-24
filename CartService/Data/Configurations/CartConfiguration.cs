using CartService.Models;
using CartService.Models.enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CartService.Data.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");

            builder.HasKey(c => c.Id);
     
       
            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd(); 

            builder.Property(c => c.UserId)
                .IsRequired(false);

            builder.Property(c => c.AnonymousId)
                .IsRequired(false)
                .HasMaxLength(100);

     
            builder.Property(c => c.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (CartStatus)Enum.Parse(typeof(CartStatus), v)) 
                .IsRequired();

            builder.Property(c => c.SubTotal)
                .HasPrecision(18, 2);

            builder.Property(c => c.DeliveryFee)
                .HasPrecision(18, 2);

            builder.Property(c => c.DeliveryAddressId)
                .IsRequired(false); 

        
            builder.HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);

     
            builder.HasIndex(c => new { c.UserId, c.Status })
                .HasFilter("[Status] = 'Active' AND UserId IS NOT NULL") 
                .IsUnique()
                .HasDatabaseName("IX_Carts_UserId_Status");

            builder.HasIndex(c => new { c.AnonymousId, c.Status })
                .HasFilter("[Status] = 'Active' AND AnonymousId IS NOT NULL") 
                .IsUnique()
                .HasDatabaseName("IX_Carts_AnonymousId_Status");

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.UpdatedAt)
                .IsRequired(false);
            builder.HasQueryFilter(c => !c.IsDeleted && c.IsActive);
        }
    }
}