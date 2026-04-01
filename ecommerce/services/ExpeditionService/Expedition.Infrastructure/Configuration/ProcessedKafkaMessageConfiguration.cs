using Expedition.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expedition.Infrastructure.Configuration;

public class ProcessedKafkaMessageConfiguration : IEntityTypeConfiguration<ProcessedKafkaMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedKafkaMessage> builder)
    {
        builder.ToTable("ProcessedKafkaMessages");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Topic)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(item => item.ConsumerGroup)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(item => new { item.Topic, item.Partition, item.Offset })
            .IsUnique();
    }
}
