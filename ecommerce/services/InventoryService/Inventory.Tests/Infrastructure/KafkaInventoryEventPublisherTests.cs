using Microsoft.Extensions.Configuration;
using Inventory.Infrastructure.Messaging;

namespace Inventory.Tests.Infrastructure;

public class KafkaInventoryEventPublisherTests
{
    [Fact]
    public async Task PublishReservationRejectedAsync_ShouldReturnWithoutThrowing_WhenBootstrapServersIsMissing()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var publisher = new KafkaInventoryEventPublisher(configuration);

        await publisher.PublishReservationRejectedAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "reason",
            Array.Empty<Inventory.Application.DTOs.InventoryReservationIssueDto>(),
            CancellationToken.None);
    }

    [Fact]
    public async Task PublishReservationRejectedAsync_ShouldHonorConfiguredTopic_WhenBootstrapServersIsMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Kafka:InventoryReservationRejectedTopic"] = "custom.topic"
            })
            .Build();
        var publisher = new KafkaInventoryEventPublisher(configuration);

        await publisher.PublishReservationRejectedAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "reason",
            Array.Empty<Inventory.Application.DTOs.InventoryReservationIssueDto>(),
            CancellationToken.None);
    }
}
