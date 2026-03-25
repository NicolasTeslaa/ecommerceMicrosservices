using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Configuration;

public class InventoryReservationConfiguration : IEntityTypeConfiguration<InventoryReservation>
{
    public void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        builder.ToTable("InventoryReservations");
        builder.HasKey(item => item.Id);

        builder.HasIndex(item => new { item.OrderId, item.ProductId })
            .IsUnique();
    }
}
