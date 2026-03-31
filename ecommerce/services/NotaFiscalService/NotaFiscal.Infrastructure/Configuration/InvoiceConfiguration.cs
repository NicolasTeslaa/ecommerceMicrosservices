using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotaFiscal.Domain.Entities;

namespace NotaFiscal.Infrastructure.Configuration;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.Series)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(invoice => invoice.AccessKey)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(invoice => invoice.XmlContent)
            .HasColumnType("longtext")
            .IsRequired();

        builder.Property(invoice => invoice.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(invoice => invoice.OrderId)
            .IsUnique();
    }
}
