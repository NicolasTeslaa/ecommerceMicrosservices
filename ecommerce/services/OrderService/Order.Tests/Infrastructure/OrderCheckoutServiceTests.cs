using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Order.Application.DTOs;
using Order.Domain.Enums;
using Order.Infrastructure.Persistence;
using Order.Tests.Support;

namespace Order.Tests.Infrastructure;

public class OrderCheckoutServiceTests
{
    [Fact]
    public async Task QueueOrderAsync_ShouldPersistOutboxMessage_WhenCommandIsValid()
    {
        var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var command = OrderTestData.CreateCommand(PaymentMethod.Credit);

        var result = await service.QueueOrderAsync(command, CancellationToken.None);

        var outbox = await dbContext.OrderProcessingOutboxMessages.SingleAsync();
        Assert.Equal(result.OrderId, outbox.OrderId);
        Assert.Equal("order.processing.requested", outbox.Topic);
    }

    [Fact]
    public async Task QueueOrderAsync_ShouldUseConfiguredTopic_WhenConfigurationOverridesDefault()
    {
        var dbContext = CreateDbContext();
        var service = CreateService(dbContext, "custom.order.processing");
        var command = OrderTestData.CreateCommand(PaymentMethod.Pix);

        await service.QueueOrderAsync(command, CancellationToken.None);

        var outbox = await dbContext.OrderProcessingOutboxMessages.SingleAsync();
        Assert.Equal("custom.order.processing", outbox.Topic);
    }

    [Fact]
    public async Task QueueOrderAsync_ShouldReturnPendingPaymentStatus()
    {
        var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var command = OrderTestData.CreateCommand(PaymentMethod.Debit);

        var result = await service.QueueOrderAsync(command, CancellationToken.None);

        Assert.Equal("pending_payment", result.Status);
        Assert.Contains("pagamento", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task QueueOrderAsync_ShouldSerializePaymentMetadataIntoOutboxPayload()
    {
        var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var command = OrderTestData.CreateCommand(PaymentMethod.Credit);

        await service.QueueOrderAsync(command, CancellationToken.None);

        var outbox = await dbContext.OrderProcessingOutboxMessages.SingleAsync();
        var payload = JsonSerializer.Deserialize<OrderProcessingRequestDto>(outbox.Payload);

        Assert.NotNull(payload);
        Assert.Equal(PaymentMethod.Credit, payload!.PaymentMethod);
        Assert.Equal("tok_123", payload.PaymentToken);
        Assert.Equal("Visa", payload.PaymentCardBrand);
        Assert.Equal("1234", payload.PaymentCardLast4);
    }

    [Fact]
    public async Task QueueOrderAsync_ShouldSerializeItemsIntoOutboxPayload()
    {
        var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var command = OrderTestData.CreateCommand(PaymentMethod.Pix);
        command.Items.Add(new Order.Application.Commands.CreateOrderItemRequest
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Produto extra",
            UnitPrice = 25m,
            Quantity = 3
        });

        await service.QueueOrderAsync(command, CancellationToken.None);

        var outbox = await dbContext.OrderProcessingOutboxMessages.SingleAsync();
        var payload = JsonSerializer.Deserialize<OrderProcessingRequestDto>(outbox.Payload);

        Assert.NotNull(payload);
        Assert.Equal(2, payload!.Items.Count);
        Assert.Contains(payload.Items, item => item.ProductName == "Produto extra" && item.Quantity == 3);
    }

    private static OrderWriteDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderWriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OrderWriteDbContext(options);
    }

    private static OrderCheckoutService CreateService(OrderWriteDbContext dbContext, string? topic = null)
    {
        var values = topic is null
            ? new Dictionary<string, string?>()
            : new Dictionary<string, string?> { ["Kafka:OrderProcessingTopic"] = topic };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new OrderCheckoutService(dbContext, configuration);
    }
}
