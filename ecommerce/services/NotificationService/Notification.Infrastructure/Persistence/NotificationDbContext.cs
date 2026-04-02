using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Infrastructure.Configuration;

namespace Notification.Infrastructure.Persistence;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<EmailNotification> EmailNotifications => Set<EmailNotification>();
    public DbSet<WhatsAppNotification> WhatsAppNotifications => Set<WhatsAppNotification>();
    public DbSet<ProcessedKafkaMessage> ProcessedKafkaMessages => Set<ProcessedKafkaMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EmailNotificationConfiguration());
        modelBuilder.ApplyConfiguration(new WhatsAppNotificationConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedKafkaMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
