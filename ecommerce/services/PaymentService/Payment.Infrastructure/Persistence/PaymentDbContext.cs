using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;
using Payment.Infrastructure.Configuration;

namespace Payment.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment.Domain.Entities.Payment> Payments => Set<Payment.Domain.Entities.Payment>();
    public DbSet<PaymentOutboxMessage> PaymentOutboxMessages => Set<PaymentOutboxMessage>();
    public DbSet<ProcessedKafkaMessage> ProcessedKafkaMessages => Set<ProcessedKafkaMessage>();
    public DbSet<ProcessedStripeWebhookEvent> ProcessedStripeWebhookEvents => Set<ProcessedStripeWebhookEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentOutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedKafkaMessageConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedStripeWebhookEventConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
