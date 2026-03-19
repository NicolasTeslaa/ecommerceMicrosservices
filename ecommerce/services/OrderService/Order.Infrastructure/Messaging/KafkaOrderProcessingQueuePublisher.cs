using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Order.Application.Interfaces;

namespace Order.Infrastructure.Messaging;

public class KafkaOrderProcessingQueuePublisher : IOrderProcessingQueuePublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaOrderProcessingQueuePublisher> _logger;

    public KafkaOrderProcessingQueuePublisher(
        IConfiguration configuration,
        ILogger<KafkaOrderProcessingQueuePublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> TryPublishAsync(Guid outboxMessageId, CancellationToken cancellationToken = default)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            _logger.LogWarning("Kafka:BootstrapServers was not configured for OrderService internal processing topic.");
            return false;
        }

        var topic = _configuration["Kafka:OrderProcessingTopic"] ?? "order.processing.requested";

        try
        {
            using var producer = new ProducerBuilder<string, string>(
                new ProducerConfig { BootstrapServers = bootstrapServers })
                .Build();

            await producer.ProduceAsync(
                topic,
                new Message<string, string>
                {
                    Key = outboxMessageId.ToString(),
                    Value = JsonSerializer.Serialize(new { OutboxMessageId = outboxMessageId })
                },
                cancellationToken);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to publish internal order processing message for outbox '{OutboxMessageId}'. The dispatcher will retry later.",
                outboxMessageId);
            return false;
        }
    }
}
