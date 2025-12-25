using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OfferService.Models;

namespace OfferService.Database.Configurations;

public class OfferConfigurations : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        // 1. Precision for Money/Math
        builder.Property(o => o.Value).HasPrecision(18, 2);

        // 2. Constraints
        builder.Property(o => o.Name).HasMaxLength(200).IsRequired();

        // 3. Global Filter (Soft Delete)
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}