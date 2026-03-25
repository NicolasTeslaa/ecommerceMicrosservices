using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Configuration;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(item => item.ProductId)
            .IsUnique();
    }
}
