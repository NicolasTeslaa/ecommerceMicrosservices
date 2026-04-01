namespace Expedition.Domain.Entities;

public class ProcessedKafkaMessage
{
    public Guid Id { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public int Partition { get; private set; }
    public long Offset { get; private set; }
    public string ConsumerGroup { get; private set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; private set; }

    private ProcessedKafkaMessage()
    {
    }

    public ProcessedKafkaMessage(string topic, int partition, long offset, string consumerGroup)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new InvalidOperationException("Topic must be provided.");
        if (string.IsNullOrWhiteSpace(consumerGroup))
            throw new InvalidOperationException("ConsumerGroup must be provided.");

        Id = Guid.NewGuid();
        Topic = topic.Trim();
        Partition = partition;
        Offset = offset;
        ConsumerGroup = consumerGroup.Trim();
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
