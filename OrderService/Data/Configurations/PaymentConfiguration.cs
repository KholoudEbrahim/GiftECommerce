using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Models;
using OrderService.Models.enums;

namespace OrderService.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder.Property(p => p.OrderId)
                .IsRequired();

            builder.Property(p => p.Method)
                .HasConversion(
                    v => v.ToString(),
                    v => (PaymentMethod)Enum.Parse(typeof(PaymentMethod), v))
                .IsRequired();

            builder.Property(p => p.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), v))
                .IsRequired();

            builder.Property(p => p.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.TransactionId)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(p => p.PaymentGatewayResponse)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.PaidAt)
                .IsRequired(false);

            builder.Property(p => p.FailureReason)
                .IsRequired(false)
                .HasMaxLength(500);

            // Stripe fields
            builder.Property(p => p.StripePaymentIntentId)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(p => p.StripeCustomerId)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(p => p.CardLastFour)
                .IsRequired(false)
                .HasMaxLength(4);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .IsRequired(false);

            builder.Property(p => p.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);
        }
    }
}
