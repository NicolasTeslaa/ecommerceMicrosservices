using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Configuration;

public class AuthUserConfiguration : IEntityTypeConfiguration<AuthUser>
{
    public void Configure(EntityTypeBuilder<AuthUser> builder)
    {
        builder.ToTable("auth_users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(user => user.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(user => user.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();
    }
}
