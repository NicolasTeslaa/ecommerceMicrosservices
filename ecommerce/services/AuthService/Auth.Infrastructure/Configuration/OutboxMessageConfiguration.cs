using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Configuration;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Topic)
            .HasColumnName("topic")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Key)
            .HasColumnName("message_key")
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

        builder.Property(message => message.OccurredOnUtc)
            .HasColumnName("occurred_on_utc")
            .IsRequired();

        builder.Property(message => message.PublishedAtUtc)
            .HasColumnName("published_at_utc");

        builder.Property(message => message.PublishAttempts)
            .HasColumnName("publish_attempts")
            .IsRequired();

        builder.Property(message => message.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(4000);

        builder.HasIndex(message => message.PublishedAtUtc);
        builder.HasIndex(message => message.OccurredOnUtc);
    }
}
