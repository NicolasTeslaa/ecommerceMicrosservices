using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Xunit;

namespace Notification.Tests.Domain;

public class WhatsAppNotificationTests
{
    [Fact]
    public void Constructor_ShouldInitializePendingNotification()
    {
        var notification = new WhatsAppNotification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "expedition.in-transit",
            "ExpeditionStatusChangedIntegrationEvent",
            "11999999999",
            "Seu pedido esta em transporte.",
            "whatsapp:expedition.in-transit:1");

        Assert.Equal(NotificationDeliveryStatus.Pending, notification.Status);
        Assert.Equal("11999999999", notification.RecipientPhoneNumber);
    }

    [Fact]
    public void RegisterFailure_ShouldUpdateStatusAndError()
    {
        var notification = new WhatsAppNotification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "expedition.in-transit",
            "ExpeditionStatusChangedIntegrationEvent",
            "11999999999",
            "Seu pedido esta em transporte.",
            "whatsapp:expedition.in-transit:1");

        notification.RegisterFailure("gateway offline");

        Assert.Equal(NotificationDeliveryStatus.Failed, notification.Status);
        Assert.Equal("gateway offline", notification.LastError);
        Assert.Equal(1, notification.AttemptCount);
    }
}
