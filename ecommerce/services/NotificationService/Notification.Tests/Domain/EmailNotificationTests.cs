using Notification.Domain.Entities;
using Notification.Domain.Enums;

namespace Notification.Tests.Domain;

public class EmailNotificationTests
{
    [Fact]
    public void Constructor_ShouldInitializePendingNotification()
    {
        var notification = new EmailNotification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "order.confirmed",
            "OrderConfirmedIntegrationEvent",
            "customer@example.com",
            "Pedido confirmado",
            "Seu pedido foi confirmado.",
            "email:order.confirmed:1");

        Assert.Equal(NotificationDeliveryStatus.Pending, notification.Status);
        Assert.Equal("customer@example.com", notification.RecipientEmail);
        Assert.Equal("Pedido confirmado", notification.Subject);
    }

    [Fact]
    public void MarkAsSent_ShouldUpdateStatusAndSentAt()
    {
        var notification = new EmailNotification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "order.confirmed",
            "OrderConfirmedIntegrationEvent",
            "customer@example.com",
            "Pedido confirmado",
            "Seu pedido foi confirmado.",
            "email:order.confirmed:1");

        notification.MarkAsSent();

        Assert.Equal(NotificationDeliveryStatus.Sent, notification.Status);
        Assert.NotNull(notification.SentAtUtc);
        Assert.Equal(1, notification.AttemptCount);
    }
}
