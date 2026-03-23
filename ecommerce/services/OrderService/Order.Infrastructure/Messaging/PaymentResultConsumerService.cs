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

        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
            throw new InvalidOperationException("Kafka:BootstrapServers was not configured for OrderService.");

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
            try
            {
                var result = consumer.Consume(stoppingToken);

                if (string.IsNullOrWhiteSpace(result?.Message?.Value))
                    continue;

                using var scope = _serviceScopeFactory.CreateScope();
                await ProcessMessageAsync(scope.ServiceProvider, result.Topic, result.Message.Value, stoppingToken);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while consuming payment results.");
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

        if (topic == (_configuration["Kafka:PaymentApprovedTopic"] ?? "payment.approved"))
        {
            var integrationEvent = JsonSerializer.Deserialize<PaymentApprovedIntegrationEvent>(payload);
            if (integrationEvent is null)
                return;

            var order = await writeDbContext.Orders.Include(item => item.Items)
                .FirstOrDefaultAsync(item => item.Id == integrationEvent.OrderId, cancellationToken);

            if (order is null || order.Status == Order.Domain.Enums.OrderStatus.PaymentRejected)
                return;

            order.MarkConfirmed();
            await writeDbContext.SaveChangesAsync(cancellationToken);
            await readModelProjector.ProjectAsync(order, cancellationToken);
            return;
        }

        var failedEvent = JsonSerializer.Deserialize<PaymentFailedIntegrationEvent>(payload);
        if (failedEvent is null)
            return;

        var failedOrder = await writeDbContext.Orders.Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == failedEvent.OrderId, cancellationToken);

        if (failedOrder is null)
            return;

        if (failedOrder.Status == Order.Domain.Enums.OrderStatus.Confirmed)
            return;

        if (!failedEvent.MaxAttemptsReached)
            return;

        failedOrder.MarkPaymentRejected(
            $"{failedEvent.FailureDetail} Limite maximo de 3 tentativas atingido. Crie um novo pedido para tentar novamente.");
        await writeDbContext.SaveChangesAsync(cancellationToken);
        await readModelProjector.ProjectAsync(failedOrder, cancellationToken);
    }
}
