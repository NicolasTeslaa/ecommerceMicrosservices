namespace Payment.Domain.Entities;

public class ProcessedStripeWebhookEvent
{
    public Guid Id { get; private set; }
    public string EventId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; private set; }

    private ProcessedStripeWebhookEvent()
    {
    }

    public ProcessedStripeWebhookEvent(string eventId, string eventType)
    {
        Id = Guid.NewGuid();
        EventId = eventId.Trim();
        EventType = eventType.Trim();
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
