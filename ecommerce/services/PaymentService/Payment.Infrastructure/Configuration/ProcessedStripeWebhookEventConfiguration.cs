using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Configuration;

public class ProcessedStripeWebhookEventConfiguration : IEntityTypeConfiguration<ProcessedStripeWebhookEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedStripeWebhookEvent> builder)
    {
        builder.ToTable("ProcessedStripeWebhookEvents");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.EventId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(item => item.EventId)
            .IsUnique();
    }
}
