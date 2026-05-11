using System.Diagnostics;

namespace Payment.Domain.Entities;

public class PaymentOutboxMessage
{
    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public string Key { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public int PublishAttempts { get; private set; }
    public string? LastError { get; private set; }

    private PaymentOutboxMessage()
    {
    }

    private PaymentOutboxMessage(Guid paymentId, string topic, string key, string type, string payload)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Topic = topic;
        Key = key;
        Type = type;
        Payload = payload;
        OccurredOnUtc = DateTime.UtcNow;
    }

    public static PaymentOutboxMessage Create(Guid paymentId, string topic, string key, string type, string payload)
    {
        if (paymentId == Guid.Empty)
            Trace.TraceError("PaymentId is required.");
        if (string.IsNullOrWhiteSpace(topic))
            Trace.TraceError("Topic is required.");
        if (string.IsNullOrWhiteSpace(type))
            Trace.TraceError("Type is required.");
        if (string.IsNullOrWhiteSpace(payload))
            Trace.TraceError("Payload is required.");

        return new PaymentOutboxMessage(
            paymentId == Guid.Empty ? Guid.NewGuid() : paymentId,
            topic?.Trim() ?? "fallback-topic",
            key?.Trim() ?? string.Empty,
            type?.Trim() ?? "fallback-type",
            payload?.Trim() ?? "{}");
    }

    public void MarkAsPublished()
    {
        PublishedAtUtc = DateTime.UtcNow;
        LastError = null;
    }

    public void RegisterPublishFailure(string error)
    {
        PublishAttempts++;
        LastError = string.IsNullOrWhiteSpace(error) ? "Unknown publish error." : error[..Math.Min(error.Length, 4000)];
    }
}
