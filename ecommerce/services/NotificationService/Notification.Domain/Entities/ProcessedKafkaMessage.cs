using System.Diagnostics;

namespace Notification.Domain.Entities;

public class ProcessedKafkaMessage
{
    public Guid Id { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public int Partition { get; private set; }
    public long Offset { get; private set; }
    public string ConsumerGroup { get; private set; } = string.Empty;
    public string MessageKey { get; private set; } = string.Empty;
    public string MessageType { get; private set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; private set; }

    private ProcessedKafkaMessage()
    {
    }

    public ProcessedKafkaMessage(string topic, int partition, long offset, string consumerGroup, string messageKey, string messageType)
    {
        Id = Guid.NewGuid();
        Topic = RequireValue(topic, nameof(topic));
        Partition = partition;
        Offset = offset;
        ConsumerGroup = RequireValue(consumerGroup, nameof(consumerGroup));
        MessageKey = messageKey?.Trim() ?? string.Empty;
        MessageType = RequireValue(messageType, nameof(messageType));
        ProcessedAtUtc = DateTime.UtcNow;
    }

    private static string RequireValue(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            Trace.TraceError("{0} is required.", paramName);

        return (value ?? string.Empty).Trim();
    }
}
