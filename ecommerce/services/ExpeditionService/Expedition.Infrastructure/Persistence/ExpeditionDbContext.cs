using Expedition.Domain.Entities;
using Expedition.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Expedition.Infrastructure.Persistence;

public class ExpeditionDbContext : DbContext
{
    public ExpeditionDbContext(DbContextOptions<ExpeditionDbContext> options)
        : base(options)
    {
    }

    public DbSet<ExpeditionOrder> ExpeditionOrders => Set<ExpeditionOrder>();
    public DbSet<ExpeditionOutboxMessage> ExpeditionOutboxMessages => Set<ExpeditionOutboxMessage>();
    public DbSet<ProcessedKafkaMessage> ProcessedKafkaMessages => Set<ProcessedKafkaMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ExpeditionOrderConfiguration());
        modelBuilder.ApplyConfiguration(new ExpeditionOutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedKafkaMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
