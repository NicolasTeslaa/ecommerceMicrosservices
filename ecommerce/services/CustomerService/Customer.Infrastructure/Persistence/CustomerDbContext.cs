using Customer.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Customer.Infrastructure.Persistence;

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    public DbSet<Customer.Domain.Entities.Customer> Customers => Set<Customer.Domain.Entities.Customer>();
    public DbSet<Customer.Domain.Entities.CustomerAddress> CustomerAddresses => Set<Customer.Domain.Entities.CustomerAddress>();
    public DbSet<Customer.Domain.Entities.ProcessedKafkaMessage> ProcessedKafkaMessages => Set<Customer.Domain.Entities.ProcessedKafkaMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerAddressConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedKafkaMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
