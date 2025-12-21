using InventoryService.Models;
using InventoryService.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.DataBase.Configurations
{
    public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
    {
        public void Configure(EntityTypeBuilder<StockTransaction> builder)
        {
            builder.HasKey(t => t.Id);

            // Global Query Filter (Soft Delete)
            builder.HasQueryFilter(x => !x.IsDeleted);

            // Properties
            builder.Property(t => t.Type)
                .HasConversion(
                    v => v.ToString(),
                    v => (StockTransactionType)Enum.Parse(typeof(StockTransactionType), v))
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(t => t.Quantity)
                .IsRequired();

            builder.Property(t => t.StockBefore)
                .IsRequired();

            builder.Property(t => t.StockAfter)
                .IsRequired();

            builder.Property(t => t.Reference)
                .HasMaxLength(100);

            builder.Property(t => t.Notes)
                .HasMaxLength(500);

            builder.Property(t => t.PerformedBy)
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(t => t.StockId);
            builder.HasIndex(t => t.Type);
            builder.HasIndex(t => t.CreatedAtUtc);
            builder.HasIndex(t => t.Reference);
        }
    }
}
