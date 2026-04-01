using Expedition.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expedition.Infrastructure.Configuration;

public class ExpeditionOutboxMessageConfiguration : IEntityTypeConfiguration<ExpeditionOutboxMessage>
{
    public void Configure(EntityTypeBuilder<ExpeditionOutboxMessage> builder)
    {
        builder.ToTable("ExpeditionOutboxMessages");

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

        builder.Property(message => message.DeduplicationKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnType("longtext")
            .IsRequired();

        builder.Property(message => message.LastError)
            .HasMaxLength(4000);

        builder.HasIndex(message => message.DeduplicationKey)
            .IsUnique();
    }
}
