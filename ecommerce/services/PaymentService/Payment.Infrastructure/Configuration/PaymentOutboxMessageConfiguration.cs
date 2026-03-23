using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Configuration;

public class PaymentOutboxMessageConfiguration : IEntityTypeConfiguration<PaymentOutboxMessage>
{
    public void Configure(EntityTypeBuilder<PaymentOutboxMessage> builder)
    {
        builder.ToTable("PaymentOutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Topic)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Key)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Type)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnType("longtext")
            .IsRequired();

        builder.Property(message => message.LastError)
            .HasMaxLength(4000);
    }
}
