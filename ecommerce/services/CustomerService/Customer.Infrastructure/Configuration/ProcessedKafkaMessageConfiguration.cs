using Customer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customer.Infrastructure.Configuration;

public class ProcessedKafkaMessageConfiguration : IEntityTypeConfiguration<ProcessedKafkaMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedKafkaMessage> builder)
    {
        builder.ToTable("processed_kafka_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Topic)
            .HasColumnName("topic")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Partition)
            .HasColumnName("partition")
            .IsRequired();

        builder.Property(message => message.Offset)
            .HasColumnName("offset")
            .IsRequired();

        builder.Property(message => message.ConsumerGroup)
            .HasColumnName("consumer_group")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.MessageKey)
            .HasColumnName("message_key")
            .HasMaxLength(200);

        builder.Property(message => message.MessageType)
            .HasColumnName("message_type")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.ProcessedAtUtc)
            .HasColumnName("processed_at_utc")
            .IsRequired();

        builder.HasIndex(message => new { message.Topic, message.Partition, message.Offset })
            .IsUnique();
    }
}
