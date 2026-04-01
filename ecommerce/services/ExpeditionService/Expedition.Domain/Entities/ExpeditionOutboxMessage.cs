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

    private ExpeditionOutboxMessage(
        Guid expeditionOrderId,
        string topic,
        string key,
        string type,
        string payload,
        string deduplicationKey)
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

    public static ExpeditionOutboxMessage Create(
        Guid expeditionOrderId,
        string topic,
        string key,
        string type,
        string payload,
        string deduplicationKey)
    {
        if (expeditionOrderId == Guid.Empty)
            throw new ArgumentException("ExpeditionOrderId is required.", nameof(expeditionOrderId));
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type is required.", nameof(type));
        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is required.", nameof(payload));
        if (string.IsNullOrWhiteSpace(deduplicationKey))
            throw new ArgumentException("DeduplicationKey is required.", nameof(deduplicationKey));

        return new ExpeditionOutboxMessage(
            expeditionOrderId,
            topic,
            key,
            type,
            payload,
            deduplicationKey);
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
