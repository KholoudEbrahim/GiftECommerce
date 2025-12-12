using CategoryService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CategoryService.DataBase.Configurations;

public class ProductOccasionConfiguration : IEntityTypeConfiguration<ProductOccasion>
{
    public void Configure(EntityTypeBuilder<ProductOccasion> builder)
    {
        // 1. Composite Primary Key
        builder.HasKey(po => new { po.ProductId, po.OccasionId });

        // 2. Relationship: Product -> ProductOccasions
        builder.HasOne(po => po.Product)
            .WithMany(p => p.ProductOccasions)
            .HasForeignKey(po => po.ProductId)
            .OnDelete(DeleteBehavior.Cascade); // If Product is hard-deleted, link goes too.

        // 3. Relationship: Occasion -> ProductOccasions
        builder.HasOne(po => po.Occasion)
            .WithMany(o => o.ProductOccasions)
            .HasForeignKey(po => po.OccasionId)
            .OnDelete(DeleteBehavior.Cascade); // If Occasion is hard-deleted, link goes too.
    }
}