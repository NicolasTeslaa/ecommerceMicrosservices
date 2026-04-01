using Expedition.Domain.Entities;
using Expedition.Infrastructure.Messaging;
using Expedition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Expedition.Tests.Infrastructure;

public class KafkaExpeditionEventPublisherTests
{
    [Fact]
    public async Task PublishStatusChangedAsync_ShouldQueueOutboxMessage()
    {
        var options = new DbContextOptionsBuilder<ExpeditionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ExpeditionDbContext(options);
        var publisher = new KafkaExpeditionEventPublisher(BuildConfiguration(), context);
        var expedition = CreateExpedition();

        await publisher.PublishStatusChangedAsync(expedition);

        var outboxMessages = context.Set<ExpeditionOutboxMessage>().Local;

        Assert.Single(outboxMessages);
        Assert.Equal("expedition.awaiting-carrier-pickup", outboxMessages.Single().Topic);
    }

    [Fact]
    public async Task PublishStatusChangedAsync_ShouldBeIdempotentPerOrderAndStatus()
    {
        var options = new DbContextOptionsBuilder<ExpeditionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ExpeditionDbContext(options);
        var publisher = new KafkaExpeditionEventPublisher(BuildConfiguration(), context);
        var expedition = CreateExpedition();

        await publisher.PublishStatusChangedAsync(expedition);
        await publisher.PublishStatusChangedAsync(expedition);

        Assert.Single(context.Set<ExpeditionOutboxMessage>().Local);
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Kafka:ExpeditionAwaitingCarrierPickupTopic"] = "expedition.awaiting-carrier-pickup",
                ["Kafka:ExpeditionPickedUpByCarrierTopic"] = "expedition.picked-up-by-carrier",
                ["Kafka:ExpeditionInTransitTopic"] = "expedition.in-transit",
                ["Kafka:ExpeditionDeliveredTopic"] = "expedition.delivered",
                ["Kafka:ExpeditionDeliveryFailedTopic"] = "expedition.delivery-failed"
            })
            .Build();
    }

    private static ExpeditionOrder CreateExpedition()
    {
        return new ExpeditionOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            123,
            "1",
            "ACCESS-KEY",
            DateTime.UtcNow);
    }
}
