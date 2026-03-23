using Payment.Domain.Entities;

namespace Payment.Tests.Domain;

public class ProcessedStripeWebhookEventTests
{
    [Fact]
    public void Constructor_ShouldTrimFields_AndSetTimestamp()
    {
        var item = new ProcessedStripeWebhookEvent(" evt_123 ", " payment_intent.succeeded ");

        Assert.Equal("evt_123", item.EventId);
        Assert.Equal("payment_intent.succeeded", item.EventType);
        Assert.NotEqual(default, item.ProcessedAtUtc);
    }
}
