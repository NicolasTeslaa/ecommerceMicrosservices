using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;
using Order.Infrastructure.Configuration;

namespace Order.Infrastructure.Persistence;

public class OrderWriteDbContext : DbContext
{
    public OrderWriteDbContext(DbContextOptions<OrderWriteDbContext> options) : base(options)
    {
    }

    public DbSet<Order.Domain.Entities.Order> Orders => Set<Order.Domain.Entities.Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderProcessingOutboxMessage> OrderProcessingOutboxMessages => Set<OrderProcessingOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderProcessingOutboxMessageConfiguration());
    }
}
