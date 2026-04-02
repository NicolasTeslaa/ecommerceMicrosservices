using Notification.Domain.Enums;

namespace Notification.Domain.Entities;

public class WhatsAppNotification
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string SourceTopic { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string RecipientPhoneNumber { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationDeliveryStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public string LastError { get; private set; } = string.Empty;
    public string DeduplicationKey { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? SentAtUtc { get; private set; }

    private WhatsAppNotification()
    {
    }

    public WhatsAppNotification(
        Guid orderId,
        Guid customerId,
        string sourceTopic,
        string eventType,
        string recipientPhoneNumber,
        string message,
        string deduplicationKey)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId;
        SourceTopic = RequireValue(sourceTopic, nameof(sourceTopic), 200);
        EventType = RequireValue(eventType, nameof(eventType), 200);
        RecipientPhoneNumber = RequireValue(recipientPhoneNumber, nameof(recipientPhoneNumber), 30);
        Message = RequireValue(message, nameof(message), 2000);
        DeduplicationKey = RequireValue(deduplicationKey, nameof(deduplicationKey), 200);
        Status = NotificationDeliveryStatus.Pending;
        AttemptCount = 0;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsSent()
    {
        AttemptCount += 1;
        Status = NotificationDeliveryStatus.Sent;
        SentAtUtc = DateTime.UtcNow;
        LastError = string.Empty;
    }

    public void RegisterFailure(string error)
    {
        AttemptCount += 1;
        Status = NotificationDeliveryStatus.Failed;
        LastError = RequireValue(error, nameof(error), 1000);
    }

    private static string RequireValue(string value, string paramName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{paramName} is required.");

        var sanitized = value.Trim();
        return sanitized.Length > maxLength ? sanitized[..maxLength] : sanitized;
    }
}
