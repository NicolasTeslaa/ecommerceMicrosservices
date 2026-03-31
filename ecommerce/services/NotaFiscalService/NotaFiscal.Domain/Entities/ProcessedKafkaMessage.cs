namespace NotaFiscal.Domain.Entities;

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
        Id = Guid.NewGuid();
        Topic = topic.Trim();
        Partition = partition;
        Offset = offset;
        ConsumerGroup = consumerGroup.Trim();
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
