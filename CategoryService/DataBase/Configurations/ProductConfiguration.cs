using CategoryService.Models;
using CategoryService.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CategoryService.DataBase.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Global Query Filter (Soft Delete)
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Precision for Price (Important for money!)
        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.Property(p => p.Discount).HasPrecision(18, 2);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();


        builder.Property(p => p.Status)
             .HasConversion(
                 convertToProviderExpression: (ProductStatus) => ProductStatus.ToString(),
                 convertFromProviderExpression: (_status) => (ProductStatus)Enum.Parse(typeof(ProductStatus), _status)).
             HasMaxLength(50);

        builder.Property(p => p.Tags)
            .HasMaxLength(1000);

        builder.Property(p => p.TotalSales)
           .HasDefaultValue(0);

        builder.Property(p => p.ViewCount)
            .HasDefaultValue(0);

        builder.Property(p => p.Rating)
            .HasPrecision(3, 2)
            .HasDefaultValue(0);

        builder.Property(p => p.RatingCount)
            .HasDefaultValue(0);

        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Price);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.TotalSales); // For best sellers query
        builder.HasIndex(p => p.ViewCount);

        builder.HasOne(p => p.Category)
           .WithMany(c => c.Products)
           .HasForeignKey(p => p.CategoryId)
           .OnDelete(DeleteBehavior.Restrict);


    }
}