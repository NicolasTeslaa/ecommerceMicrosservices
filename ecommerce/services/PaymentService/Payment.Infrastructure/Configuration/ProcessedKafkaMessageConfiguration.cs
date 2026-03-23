using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Configuration;

public class ProcessedKafkaMessageConfiguration : IEntityTypeConfiguration<ProcessedKafkaMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedKafkaMessage> builder)
    {
        builder.ToTable("ProcessedKafkaMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Topic)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.ConsumerGroup)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.MessageKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.MessageType)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(message => new { message.Topic, message.Partition, message.Offset })
            .IsUnique();
    }
}
