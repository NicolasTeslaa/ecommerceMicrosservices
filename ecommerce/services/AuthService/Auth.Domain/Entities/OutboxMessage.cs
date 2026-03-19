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
            throw new ArgumentException("Topic is required.", nameof(topic));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type is required.", nameof(type));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is required.", nameof(payload));

        return new OutboxMessage(topic, key, type, payload);
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
