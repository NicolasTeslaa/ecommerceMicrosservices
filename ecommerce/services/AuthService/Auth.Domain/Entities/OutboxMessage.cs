using System.Diagnostics;

namespace Auth.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public string Key { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public int PublishAttempts { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage()
    {
    }

    private OutboxMessage(string topic, string key, string type, string payload)
    {
        Id = Guid.NewGuid();
        Topic = topic;
        Key = key;
        Type = type;
        Payload = payload;
        OccurredOnUtc = DateTime.UtcNow;
    }

    public static OutboxMessage Create(string topic, string key, string type, string payload)
    {
        if (string.IsNullOrWhiteSpace(topic))
            Trace.TraceError("Outbox topic is required.");
        if (string.IsNullOrWhiteSpace(type))
            Trace.TraceError("Outbox type is required.");
        if (string.IsNullOrWhiteSpace(payload))
            Trace.TraceError("Outbox payload is required.");

        return new OutboxMessage(
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
        LastError = string.IsNullOrWhiteSpace(error)
            ? "Unknown publish error."
            : error[..Math.Min(error.Length, 4000)];
    }
}
