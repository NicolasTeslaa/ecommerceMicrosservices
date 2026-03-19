using Customer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customer.Infrastructure.Configuration;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("customer_addresses");

        builder.HasKey(address => address.Id);

        builder.Property(address => address.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(address => address.Label)
            .HasColumnName("label")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(address => address.RecipientName)
            .HasColumnName("recipient_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(address => address.Street)
            .HasColumnName("street")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(address => address.Number)
            .HasColumnName("number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(address => address.Complement)
            .HasColumnName("complement")
            .HasMaxLength(100);

        builder.Property(address => address.Neighborhood)
            .HasColumnName("neighborhood")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(address => address.City)
            .HasColumnName("city")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(address => address.State)
            .HasColumnName("state")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(address => address.ZipCode)
            .HasColumnName("zip_code")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(address => address.Country)
            .HasColumnName("country")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(address => address.Reference)
            .HasColumnName("reference")
            .HasMaxLength(200);

        builder.Property(address => address.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.Property(address => address.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(address => address.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasIndex(address => new { address.CustomerId, address.IsDefault });
    }
}
