using Inventory.Domain.Entities;
using Inventory.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();
    public DbSet<ProcessedKafkaMessage> ProcessedKafkaMessages => Set<ProcessedKafkaMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InventoryItemConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryReservationConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedKafkaMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
