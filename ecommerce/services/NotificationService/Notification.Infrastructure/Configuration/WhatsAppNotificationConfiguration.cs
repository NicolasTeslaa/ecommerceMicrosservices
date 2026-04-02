using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Configuration;

public class WhatsAppNotificationConfiguration : IEntityTypeConfiguration<WhatsAppNotification>
{
    public void Configure(EntityTypeBuilder<WhatsAppNotification> builder)
    {
        builder.ToTable("WhatsAppNotifications");
        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.SourceTopic).HasMaxLength(200).IsRequired();
        builder.Property(notification => notification.EventType).HasMaxLength(200).IsRequired();
        builder.Property(notification => notification.RecipientPhoneNumber).HasMaxLength(30).IsRequired();
        builder.Property(notification => notification.Message).HasMaxLength(2000).IsRequired();
        builder.Property(notification => notification.DeduplicationKey).HasMaxLength(200).IsRequired();
        builder.Property(notification => notification.LastError).HasMaxLength(1000).IsRequired();
        builder.Property(notification => notification.Status)
            .HasConversion(
                value => value.ToString(),
                value => Enum.Parse<NotificationDeliveryStatus>(value))
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(notification => notification.OrderId);
        builder.HasIndex(notification => notification.DeduplicationKey).IsUnique();
    }
}
