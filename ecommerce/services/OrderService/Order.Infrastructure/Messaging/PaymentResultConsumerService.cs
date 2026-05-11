using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Order.Application.Interfaces;
using Order.Infrastructure.Persistence;

namespace Order.Infrastructure.Messaging;

public class PaymentResultConsumerService : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentResultConsumerService> _logger;

    public PaymentResultConsumerService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<PaymentResultConsumerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var bootstrapServers = _configuration["Kafka:BootstrapServers"];

                if (string.IsNullOrWhiteSpace(bootstrapServers))
                {
                    _logger.LogWarning("Order payment-result consumer is waiting because Kafka:BootstrapServers was not configured.");
                    await Task.Delay(RetryDelay, stoppingToken);
                    continue;
                }

                var approvedTopic = _configuration["Kafka:PaymentApprovedTopic"] ?? "payment.approved";
                var failedTopic = _configuration["Kafka:PaymentFailedTopic"] ?? "payment.failed";
                var groupId = _configuration["Kafka:PaymentResultConsumerGroup"] ?? "order-payment-results";

                using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
                {
                    BootstrapServers = bootstrapServers,
                    GroupId = groupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    AllowAutoCreateTopics = true,
                    EnableAutoCommit = false
                }).Build();

                consumer.Subscribe([approvedTopic, failedTopic]);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(stoppingToken);

                    if (string.IsNullOrWhiteSpace(result?.Message?.Value))
                        continue;

                    using var scope = _serviceScopeFactory.CreateScope();
                    await ProcessMessageAsync(scope.ServiceProvider, result.Topic, result.Message.Value, stoppingToken);
                    consumer.Commit(result);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while consuming payment results.");
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(
        IServiceProvider serviceProvider,
        string topic,
        string payload,
        CancellationToken cancellationToken)
    {
        var writeDbContext = serviceProvider.GetRequiredService<OrderWriteDbContext>();
        var readModelProjector = serviceProvider.GetRequiredService<IOrderReadModelProjector>();
        var eventPublisher = serviceProvider.GetRequiredService<IOrderEventPublisher>();

        if (topic == (_configuration["Kafka:PaymentApprovedTopic"] ?? "payment.approved"))
        {
            var integrationEvent = JsonSerializer.Deserialize<PaymentApprovedIntegrationEvent>(payload);
            if (integrationEvent is null)
            {
                _logger.LogWarning("Order payment-result consumer ignored a payment.approved message because it could not deserialize the payload.");
                return;
            }

            var order = await writeDbContext.Orders.Include(item => item.Items)
                .FirstOrDefaultAsync(item => item.Id == integrationEvent.OrderId, cancellationToken);

            if (order is null
                || order.Status == Order.Domain.Enums.OrderStatus.PaymentRejected
                || order.Status == Order.Domain.Enums.OrderStatus.Cancelled)
                return;

            order.MarkConfirmed();
            await writeDbContext.SaveChangesAsync(cancellationToken);
            await readModelProjector.ProjectAsync(order, cancellationToken);
            await eventPublisher.PublishOrderConfirmedAsync(order, cancellationToken);
            return;
        }

        var failedEvent = JsonSerializer.Deserialize<PaymentFailedIntegrationEvent>(payload);
        if (failedEvent is null)
        {
            _logger.LogWarning("Order payment-result consumer ignored a payment.failed message because it could not deserialize the payload.");
            return;
        }

        var failedOrder = await writeDbContext.Orders.Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == failedEvent.OrderId, cancellationToken);

        if (failedOrder is null)
            return;

        if (failedOrder.Status == Order.Domain.Enums.OrderStatus.Confirmed
            || failedOrder.Status == Order.Domain.Enums.OrderStatus.Cancelled)
            return;

        if (!failedEvent.MaxAttemptsReached)
            return;

        failedOrder.MarkPaymentRejected(
            $"{failedEvent.FailureDetail} Limite maximo de 3 tentativas atingido. Crie um novo pedido para tentar novamente.");
        await writeDbContext.SaveChangesAsync(cancellationToken);
        await readModelProjector.ProjectAsync(failedOrder, cancellationToken);
    }
}
