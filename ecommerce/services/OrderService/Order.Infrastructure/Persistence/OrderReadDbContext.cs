using Microsoft.EntityFrameworkCore;
using Order.Application.ReadModels;
using Order.Infrastructure.Configuration;

namespace Order.Infrastructure.Persistence;

public class OrderReadDbContext : DbContext
{
    public OrderReadDbContext(DbContextOptions<OrderReadDbContext> options) : base(options)
    {
    }

    public DbSet<OrderReadModel> Orders => Set<OrderReadModel>();
    public DbSet<OrderItemReadModel> OrderItems => Set<OrderItemReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderReadModelConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemReadModelConfiguration());
    }
}
