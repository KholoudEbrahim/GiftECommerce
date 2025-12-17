using InventoryService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace InventoryService.DataBase.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasKey(s => s.Id);

            // Global Query Filter (Soft Delete)
            builder.HasQueryFilter(x => !x.IsDeleted);

            // Properties
            builder.Property(s => s.ProductId)
                   .IsRequired();

            builder.Property(s => s.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(s => s.CurrentStock)
                   .IsRequired();

            builder.Property(s => s.MinStock)
                   .IsRequired();

            builder.Property(s => s.MaxStock)
                   .IsRequired();

            // Indexes
            builder.HasIndex(s => s.ProductId)
                .IsUnique();

            builder.HasIndex(s => s.CurrentStock);

            // Relationships
            builder.HasMany(s => s.Transactions)
                .WithOne(t => t.Stock)
                .HasForeignKey(t => t.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
