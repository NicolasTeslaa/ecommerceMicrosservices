using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Payment.Infrastructure.Configuration;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment.Domain.Entities.Payment>
{
    public void Configure(EntityTypeBuilder<Payment.Domain.Entities.Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(payment => payment.StripePaymentIntentId)
            .HasMaxLength(200);

        builder.Property(payment => payment.StripeClientSecret)
            .HasMaxLength(500);

        builder.Property(payment => payment.StripePaymentMethodId)
            .HasMaxLength(200);

        builder.Property(payment => payment.FailureDetail)
            .HasMaxLength(2000);

        builder.Property(payment => payment.AttemptCount)
            .IsRequired();

        builder.HasIndex(payment => payment.OrderId)
            .IsUnique();

        builder.HasIndex(payment => payment.StripePaymentIntentId)
            .IsUnique();
    }
}
