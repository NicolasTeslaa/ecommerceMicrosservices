using System.Text.Json;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;

namespace Catalog.Infrastructure.Messaging;

public class KafkaCatalogProductIntegrationEventPublisher : ICatalogProductIntegrationEventPublisher
{
    private readonly IConfiguration _configuration;

    public KafkaCatalogProductIntegrationEventPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task PublishProductCreatedAsync(
        Product product,
        int stockDelta,
        CancellationToken cancellationToken = default)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(bootstrapServers))
            return;

        var topic = _configuration["Kafka:CatalogProductCreatedTopic"] ?? "catalog.product-created";
        var integrationEvent = new CatalogProductCreatedIntegrationEvent
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Price = product.Price,
            InitialStockQuantity = stockDelta,
            Active = product.Active,
            OccurredAtUtc = DateTime.UtcNow
        };

        using var producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        }).Build();

        await producer.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = product.Id.ToString(),
                Value = JsonSerializer.Serialize(integrationEvent)
            },
            cancellationToken);
    }
}
