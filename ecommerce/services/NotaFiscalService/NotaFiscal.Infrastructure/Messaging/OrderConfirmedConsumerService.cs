using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NotaFiscal.Infrastructure.Messaging;

public class OrderConfirmedConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderConfirmedConsumerService> _logger;

    public OrderConfirmedConsumerService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<OrderConfirmedConsumerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await ConsumeAsync(
            _configuration["Kafka:OrderConfirmedTopic"] ?? "order.confirmed",
            _configuration["Kafka:OrderConfirmedConsumerGroup"] ?? "nota-fiscal-order-confirmed",
            stoppingToken);
    }

    private async Task ConsumeAsync(string topic, string groupId, CancellationToken stoppingToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
            throw new InvalidOperationException("Kafka:BootstrapServers was not configured for NotaFiscalService.");

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        }).Build();

        consumer.Subscribe(topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);

                if (string.IsNullOrWhiteSpace(result?.Message?.Value))
                    continue;

                var integrationEvent = JsonSerializer.Deserialize<OrderConfirmedIntegrationEvent>(result.Message.Value);
                if (integrationEvent is null)
                    continue;

                using var scope = _serviceScopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OrderConfirmedMessageProcessor>();
                await processor.ProcessAsync(
                    integrationEvent,
                    result.Topic,
                    result.Partition.Value,
                    result.Offset.Value,
                    groupId,
                    stoppingToken);

                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while consuming order.confirmed.");
            }
        }
    }
}
