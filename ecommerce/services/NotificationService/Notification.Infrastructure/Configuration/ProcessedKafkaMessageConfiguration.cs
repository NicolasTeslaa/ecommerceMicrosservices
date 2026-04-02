using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Configuration;

public class ProcessedKafkaMessageConfiguration : IEntityTypeConfiguration<ProcessedKafkaMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedKafkaMessage> builder)
    {
        builder.ToTable("ProcessedKafkaMessages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.Topic).HasMaxLength(255).IsRequired();
        builder.Property(message => message.ConsumerGroup).HasMaxLength(255).IsRequired();
        builder.Property(message => message.MessageKey).HasMaxLength(255).IsRequired();
        builder.Property(message => message.MessageType).HasMaxLength(255).IsRequired();

        builder.HasIndex(message => new { message.Topic, message.Partition, message.Offset }).IsUnique();
    }
}
