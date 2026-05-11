using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.Messaging;

public class PaymentApprovedConsumerService : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentApprovedConsumerService> _logger;

    public PaymentApprovedConsumerService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<PaymentApprovedConsumerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await ConsumeAsync(_configuration["Kafka:PaymentApprovedTopic"] ?? "payment.approved", "inventory-payment-approved", stoppingToken);
    }

    private async Task ConsumeAsync(string topic, string groupId, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var bootstrapServers = _configuration["Kafka:BootstrapServers"];
                if (string.IsNullOrWhiteSpace(bootstrapServers))
                {
                    _logger.LogWarning("Inventory payment-approved consumer is waiting because Kafka:BootstrapServers was not configured.");
                    await Task.Delay(RetryDelay, stoppingToken);
                    continue;
                }

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
                    var result = consumer.Consume(stoppingToken);
                    if (string.IsNullOrWhiteSpace(result?.Message?.Value))
                        continue;

                    using var scope = _serviceScopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<PaymentApprovedMessageProcessor>();

                    var integrationEvent = JsonSerializer.Deserialize<PaymentApprovedIntegrationEvent>(result.Message.Value);
                    if (integrationEvent is null)
                    {
                        _logger.LogWarning("Inventory payment-approved consumer ignored a message because it could not deserialize PaymentApprovedIntegrationEvent.");
                        consumer.Commit(result);
                        continue;
                    }

                    var processed = await processor.ProcessAsync(
                        integrationEvent,
                        result.Topic,
                        result.Partition.Value,
                        result.Offset.Value,
                        groupId,
                        stoppingToken);

                    if (processed)
                        consumer.Commit(result);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while consuming payment.approved.");
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }
}
