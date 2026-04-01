using Expedition.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expedition.Infrastructure.Configuration;

public class ExpeditionOrderConfiguration : IEntityTypeConfiguration<ExpeditionOrder>
{
    public void Configure(EntityTypeBuilder<ExpeditionOrder> builder)
    {
        builder.ToTable("ExpeditionOrders");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.InvoiceSeries)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(item => item.InvoiceAccessKey)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(item => item.Status)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(item => item.FailureReason)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(item => item.FailureDetails)
            .HasMaxLength(512)
            .IsRequired();

        builder.HasIndex(item => item.OrderId)
            .IsUnique();

        builder.HasIndex(item => item.InvoiceId)
            .IsUnique();
    }
}
