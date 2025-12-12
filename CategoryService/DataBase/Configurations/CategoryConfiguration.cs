using CategoryService.Models;
using CategoryService.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CategoryService.DataBase.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Global Query Filter (Soft Delete)
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Optional: Explicitly configure the One-to-Many if not done in Product
        builder.HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId);

        // Basic Property Configs (Optional but good practice)
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Status).HasMaxLength(20);


        builder.Property(c => c.Status)
       .HasConversion(
                convertToProviderExpression: (CategoryStatus) => CategoryStatus.ToString(),
                convertFromProviderExpression: (_status) => (CategoryStatus)Enum.Parse(typeof(CategoryStatus), _status)
                  ).
               HasMaxLength(50);


        builder.HasIndex(c => c.Name);
    }
}