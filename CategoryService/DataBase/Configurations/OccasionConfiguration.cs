using CategoryService.Models;
using CategoryService.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CategoryService.DataBase.Configurations;

public class OccasionConfiguration : IEntityTypeConfiguration<Occasion>
{
    public void Configure(EntityTypeBuilder<Occasion> builder)
    {
        // Global Query Filter (Soft Delete)
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.Property(o => o.Name).HasMaxLength(100).IsRequired();

       builder.Property(o => o.Status)
      .HasConversion(
               convertToProviderExpression: (OccasionStatus) => OccasionStatus.ToString(),
               convertFromProviderExpression: (_status) => (OccasionStatus)Enum.Parse(typeof(OccasionStatus), _status)
                 ).
              HasMaxLength(50);
    }
}