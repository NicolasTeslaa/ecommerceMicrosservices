using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Configuration;

public class OrderProcessingOutboxMessageConfiguration : IEntityTypeConfiguration<OrderProcessingOutboxMessage>
{
    public void Configure(EntityTypeBuilder<OrderProcessingOutboxMessage> builder)
    {
        builder.ToTable("order_processing_outbox");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(message => message.Topic)
            .HasColumnName("topic")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Type)
            .HasColumnName("type")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnName("payload")
            .HasColumnType("longtext")
            .IsRequired();

        builder.Property(message => message.RequestedAtUtc)
            .HasColumnName("requested_at_utc")
            .IsRequired();

        builder.Property(message => message.DispatchedAtUtc)
            .HasColumnName("dispatched_at_utc");

        builder.Property(message => message.ProcessedAtUtc)
            .HasColumnName("processed_at_utc");

        builder.Property(message => message.LastDispatchAttemptAtUtc)
            .HasColumnName("last_dispatch_attempt_at_utc");

        builder.Property(message => message.DispatchAttempts)
            .HasColumnName("dispatch_attempts")
            .IsRequired();

        builder.Property(message => message.LastDispatchError)
            .HasColumnName("last_dispatch_error")
            .HasMaxLength(4000);

        builder.Property(message => message.ProcessingAttempts)
            .HasColumnName("processing_attempts")
            .IsRequired();

        builder.Property(message => message.LastProcessingError)
            .HasColumnName("last_processing_error")
            .HasMaxLength(4000);

        builder.HasIndex(message => message.OrderId)
            .IsUnique();

        builder.HasIndex(message => message.DispatchedAtUtc);
        builder.HasIndex(message => message.ProcessedAtUtc);
        builder.HasIndex(message => message.RequestedAtUtc);
    }
}
