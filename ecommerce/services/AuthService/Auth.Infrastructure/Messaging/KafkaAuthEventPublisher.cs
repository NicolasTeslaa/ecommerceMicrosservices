using System.Text.Json;
using Auth.Application.Interfaces;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.Messaging;

public class KafkaAuthEventPublisher : IAuthEventPublisher
{
    private readonly IConfiguration _configuration;

    public KafkaAuthEventPublisher(IConfiguration configuration) => _configuration = configuration;

    public async Task PublishUserRegisteredAsync(UserRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"]
        };

        var topic = _configuration["Kafka:UserRegisteredTopic"] ?? "auth.user-registered";
        var payload = JsonSerializer.Serialize(integrationEvent);

        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

        await producer.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = integrationEvent.CustomerId.ToString(),
                Value = payload
            },
            cancellationToken);
    }
}
