using Customer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customer.Infrastructure.Configuration;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer.Domain.Entities.Customer>
{
    public void Configure(EntityTypeBuilder<Customer.Domain.Entities.Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.AuthUserId)
            .HasColumnName("auth_user_id")
            .IsRequired();

        builder.Property(customer => customer.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasColumnName("email")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(customer => customer.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(customer => customer.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Navigation(customer => customer.Addresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(customer => customer.Addresses)
            .WithOne()
            .HasForeignKey(address => address.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(customer => customer.AuthUserId)
            .IsUnique();

        builder.HasIndex(customer => customer.Email)
            .IsUnique();
    }
}
