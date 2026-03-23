using System.Text.Json;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Payment.Domain.Enums;
using Payment.Domain.Entities;
using Payment.Infrastructure.Messaging;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Payment.Tests.Infrastructure;

public class KafkaPaymentEventPublisherTests
{
    [Fact]
    public async Task PublishApprovedAsync_ShouldQueueApprovedOutboxMessage()
    {
        await using var context = CreateDbContext();
        var publisher = new KafkaPaymentEventPublisher(BuildConfiguration(("Kafka:PaymentApprovedTopic", "approved-topic")), context);
        var payment = PaymentTestData.CreatePaymentWithIntent();

        await publisher.PublishApprovedAsync(payment);

        var message = context.ChangeTracker.Entries<PaymentOutboxMessage>().Single().Entity;
        Assert.Equal("approved-topic", message.Topic);
        var payload = JsonSerializer.Deserialize<PaymentApprovedIntegrationEvent>(message.Payload);
        Assert.Equal(payment.OrderId, payload!.OrderId);
    }

    [Fact]
    public async Task PublishFailedAsync_ShouldQueueFailedOutboxMessage()
    {
        await using var context = CreateDbContext();
        var publisher = new KafkaPaymentEventPublisher(BuildConfiguration(("Kafka:PaymentFailedTopic", "failed-topic")), context);
        var payment = PaymentTestData.CreatePaymentWithIntent();
        payment.MarkFailed(PaymentFailureReason.CardDeclined, "erro");

        await publisher.PublishFailedAsync(payment);

        var message = context.ChangeTracker.Entries<PaymentOutboxMessage>().Single().Entity;
        Assert.Equal("failed-topic", message.Topic);
        var payload = JsonSerializer.Deserialize<PaymentFailedIntegrationEvent>(message.Payload);
        Assert.Equal(payment.AttemptCount, payload!.AttemptCount);
        Assert.Equal("erro", payload.FailureDetail);
    }

    [Fact]
    public async Task PublishApprovedAsync_ShouldUseDefaultTopic_WhenConfigIsMissing()
    {
        await using var context = CreateDbContext();
        var publisher = new KafkaPaymentEventPublisher(BuildConfiguration(), context);
        var payment = PaymentTestData.CreatePaymentWithIntent();

        await publisher.PublishApprovedAsync(payment);

        Assert.Equal("payment.approved", context.ChangeTracker.Entries<PaymentOutboxMessage>().Single().Entity.Topic);
    }

    private static PaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PaymentDbContext(options);
    }

    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] entries)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(entries.ToDictionary(x => x.Key, x => x.Value))
            .Build();
    }
}
