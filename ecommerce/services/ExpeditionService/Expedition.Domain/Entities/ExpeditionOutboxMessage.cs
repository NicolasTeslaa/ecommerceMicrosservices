using System.Diagnostics;

namespace Expedition.Domain.Entities;

public class ExpeditionOutboxMessage
{
    public Guid Id { get; private set; }
    public Guid ExpeditionOrderId { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public string Key { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string DeduplicationKey { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public int PublishAttempts { get; private set; }
    public string? LastError { get; private set; }

    private ExpeditionOutboxMessage()
    {
    }

    private ExpeditionOutboxMessage(Guid expeditionOrderId, string topic, string key, string type, string payload, string deduplicationKey)
    {
        Id = Guid.NewGuid();
        ExpeditionOrderId = expeditionOrderId;
        Topic = topic;
        Key = key;
        Type = type;
        Payload = payload;
        DeduplicationKey = deduplicationKey;
        OccurredOnUtc = DateTime.UtcNow;
    }

    public static ExpeditionOutboxMessage Create(Guid expeditionOrderId, string topic, string key, string type, string payload, string deduplicationKey)
    {
        if (expeditionOrderId == Guid.Empty)
            Trace.TraceError("ExpeditionOrderId is required.");
        if (string.IsNullOrWhiteSpace(topic))
            Trace.TraceError("Topic is required.");
        if (string.IsNullOrWhiteSpace(type))
            Trace.TraceError("Type is required.");
        if (string.IsNullOrWhiteSpace(payload))
            Trace.TraceError("Payload is required.");
        if (string.IsNullOrWhiteSpace(deduplicationKey))
            Trace.TraceError("DeduplicationKey is required.");

        return new ExpeditionOutboxMessage(
            expeditionOrderId == Guid.Empty ? Guid.NewGuid() : expeditionOrderId,
            topic?.Trim() ?? "fallback-topic",
            key?.Trim() ?? string.Empty,
            type?.Trim() ?? "fallback-type",
            payload?.Trim() ?? "{}",
            deduplicationKey?.Trim() ?? Guid.NewGuid().ToString("N"));
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
