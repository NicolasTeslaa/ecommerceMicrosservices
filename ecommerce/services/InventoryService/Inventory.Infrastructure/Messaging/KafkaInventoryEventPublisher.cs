using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Inventory.Infrastructure.Messaging;

public class KafkaInventoryEventPublisher : IInventoryEventPublisher
{
    private readonly IConfiguration _configuration;

    public KafkaInventoryEventPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task PublishReservationRejectedAsync(
        Guid orderId,
        Guid customerId,
        string reason,
        IReadOnlyCollection<InventoryReservationIssueDto> issues,
        CancellationToken cancellationToken = default)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
            return;

        var topic = _configuration["Kafka:InventoryReservationRejectedTopic"] ?? "inventory.reservation-rejected";
        var integrationEvent = new InventoryReservationRejectedIntegrationEvent
        {
            OrderId = orderId,
            CustomerId = customerId,
            Reason = reason,
            RejectedAtUtc = DateTime.UtcNow,
            Items = issues.Select(issue => new InventoryReservationRejectedItemIntegrationEvent
            {
                ProductId = issue.ProductId,
                RequestedQuantity = issue.RequestedQuantity,
                AvailableQuantity = issue.AvailableQuantity,
                Reason = issue.Reason
            }).ToArray()
        };

        using var producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        }).Build();

        await producer.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = orderId.ToString(),
                Value = JsonSerializer.Serialize(integrationEvent)
            },
            cancellationToken);
    }
}
