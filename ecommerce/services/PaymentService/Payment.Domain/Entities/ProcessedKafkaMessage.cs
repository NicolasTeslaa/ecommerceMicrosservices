namespace Payment.Domain.Entities;

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
        Topic = topic.Trim();
        Partition = partition;
        Offset = offset;
        ConsumerGroup = consumerGroup.Trim();
        MessageKey = messageKey?.Trim() ?? string.Empty;
        MessageType = messageType.Trim();
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
