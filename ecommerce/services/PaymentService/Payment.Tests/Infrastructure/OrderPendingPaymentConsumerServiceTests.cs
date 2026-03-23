using System.Reflection;
using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Payment.Application.Interfaces;
using Payment.Application.Models;
using Payment.Domain.Enums;
using Payment.Infrastructure.Messaging;
using Payment.Infrastructure.Persistence;

namespace Payment.Tests.Infrastructure;

public class OrderPendingPaymentConsumerServiceTests
{
    [Fact]
    public async Task ProcessMessageAsync_ShouldCreateIntent_ForCardPayments()
    {
        await using var context = CreateDbContext();
        var services = new ServiceCollection();
        var stripeGateway = new Mock<IStripePaymentGateway>();
        var eventPublisher = new Mock<IPaymentEventPublisher>();
        var notifier = new Mock<IPaymentRealtimeNotifier>();
        stripeGateway.Setup(item => item.CreatePaymentIntentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), "brl", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StripePaymentIntentResult
            {
                PaymentIntentId = "pi_created",
                ClientSecret = "secret_created",
                PaymentMethodId = "pm_created"
            });

        services.AddSingleton<PaymentDbContext>(context);
        services.AddScoped<IPaymentRepository>(_ => new PaymentRepository(context));
        services.AddSingleton(stripeGateway.Object);
        services.AddSingleton(eventPublisher.Object);
        services.AddSingleton(notifier.Object);

        var service = new OrderPendingPaymentConsumerService(
            Mock.Of<IServiceScopeFactory>(),
            BuildConfiguration(),
            Mock.Of<ILogger<OrderPendingPaymentConsumerService>>());

        await InvokeProcessMessageAsync(service, services.BuildServiceProvider(), CreateConsumeResult(BuildOrderCreatedEvent("credit")), "group-a");

        var payment = await context.Payments.SingleAsync();
        Assert.Equal(PaymentStatus.PendingConfirmation, payment.Status);
        Assert.Equal("pi_created", payment.StripePaymentIntentId);
        Assert.Single(context.ProcessedKafkaMessages);
        notifier.Verify(item => item.NotifyUpdatedAsync(payment.OrderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldFailPixPayments_AndPublishFailure()
    {
        await using var context = CreateDbContext();
        var services = new ServiceCollection();
        var eventPublisher = new Mock<IPaymentEventPublisher>();
        var notifier = new Mock<IPaymentRealtimeNotifier>();

        services.AddSingleton<PaymentDbContext>(context);
        services.AddScoped<IPaymentRepository>(_ => new PaymentRepository(context));
        services.AddSingleton(Mock.Of<IStripePaymentGateway>());
        services.AddSingleton(eventPublisher.Object);
        services.AddSingleton(notifier.Object);

        var service = new OrderPendingPaymentConsumerService(
            Mock.Of<IServiceScopeFactory>(),
            BuildConfiguration(),
            Mock.Of<ILogger<OrderPendingPaymentConsumerService>>());

        await InvokeProcessMessageAsync(service, services.BuildServiceProvider(), CreateConsumeResult(BuildOrderCreatedEvent("pix")), "group-a");

        var payment = await context.Payments.SingleAsync();
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal(PaymentFailureReason.InvalidPaymentMethod, payment.FailureReason);
        eventPublisher.Verify(item => item.PublishFailedAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldSkipAlreadyProcessedMessages()
    {
        await using var context = CreateDbContext();
        await context.ProcessedKafkaMessages.AddAsync(new Payment.Domain.Entities.ProcessedKafkaMessage("order.pending-payment", 0, 5, "group-a", "key", nameof(OrderCreatedIntegrationEvent)));
        await context.SaveChangesAsync();

        var provider = new ServiceCollection()
            .AddSingleton(context)
            .AddScoped<IPaymentRepository>(_ => new PaymentRepository(context))
            .AddSingleton(Mock.Of<IStripePaymentGateway>())
            .AddSingleton(Mock.Of<IPaymentEventPublisher>())
            .AddSingleton(Mock.Of<IPaymentRealtimeNotifier>())
            .BuildServiceProvider();

        var service = new OrderPendingPaymentConsumerService(
            Mock.Of<IServiceScopeFactory>(),
            BuildConfiguration(),
            Mock.Of<ILogger<OrderPendingPaymentConsumerService>>());

        await InvokeProcessMessageAsync(service, provider, CreateConsumeResult(BuildOrderCreatedEvent("credit"), offset: 5), "group-a");

        Assert.Empty(context.Payments);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldThrow_WhenStripeIntentCreationFails()
    {
        await using var context = CreateDbContext();
        var services = new ServiceCollection();
        var stripeGateway = new Mock<IStripePaymentGateway>();
        stripeGateway.Setup(item => item.CreatePaymentIntentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), "brl", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("stripe down"));

        services.AddSingleton<PaymentDbContext>(context);
        services.AddScoped<IPaymentRepository>(_ => new PaymentRepository(context));
        services.AddSingleton(stripeGateway.Object);
        services.AddSingleton(Mock.Of<IPaymentEventPublisher>());
        services.AddSingleton(Mock.Of<IPaymentRealtimeNotifier>());

        var provider = services.BuildServiceProvider();
        var service = new OrderPendingPaymentConsumerService(
            Mock.Of<IServiceScopeFactory>(),
            BuildConfiguration(),
            Mock.Of<ILogger<OrderPendingPaymentConsumerService>>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            InvokeProcessMessageAsync(service, provider, CreateConsumeResult(BuildOrderCreatedEvent("credit")), "group-a"));

        Assert.Empty(context.ProcessedKafkaMessages);
    }

    private static async Task InvokeProcessMessageAsync(
        OrderPendingPaymentConsumerService service,
        IServiceProvider provider,
        ConsumeResult<string, string> result,
        string consumerGroup)
    {
        var method = typeof(OrderPendingPaymentConsumerService).GetMethod("ProcessMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

        try
        {
            var task = (Task)method.Invoke(service, new object[] { provider, result, consumerGroup, CancellationToken.None })!;
            await task;
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            throw exception.InnerException;
        }
    }

    private static ConsumeResult<string, string> CreateConsumeResult(OrderCreatedIntegrationEvent integrationEvent, long offset = 1)
    {
        return new ConsumeResult<string, string>
        {
            Topic = "order.pending-payment",
            Partition = new Partition(0),
            Offset = new Offset(offset),
            Message = new Message<string, string>
            {
                Key = integrationEvent.OrderId.ToString(),
                Value = JsonSerializer.Serialize(integrationEvent)
            }
        };
    }

    private static OrderCreatedIntegrationEvent BuildOrderCreatedEvent(string paymentMethod)
    {
        return new OrderCreatedIntegrationEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerAddressId = Guid.NewGuid(),
            CustomerEmail = "customer@example.com",
            PaymentMethod = paymentMethod,
            TotalAmount = 110m,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static PaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PaymentDbContext(options);
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Stripe:Currency"] = "brl"
            })
            .Build();
    }
}
