using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Expedition.Application.Interfaces;
using Expedition.Domain.Entities;
using Expedition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Expedition.Infrastructure.Messaging;

public class InvoiceIssuedConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InvoiceIssuedConsumerService> _logger;

    public InvoiceIssuedConsumerService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<InvoiceIssuedConsumerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await ConsumeAsync(stoppingToken);
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(bootstrapServers))
            throw new InvalidOperationException("Kafka:BootstrapServers was not configured for ExpeditionService.");

        var topic = _configuration["Kafka:InvoiceIssuedTopic"] ?? "invoice.issued";
        var groupId = _configuration["Kafka:InvoiceIssuedConsumerGroup"] ?? "expedition-invoice-issued";

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

                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ExpeditionDbContext>();
                var repository = scope.ServiceProvider.GetRequiredService<IExpeditionRepository>();
                var eventPublisher = scope.ServiceProvider.GetRequiredService<IExpeditionEventPublisher>();

                var alreadyProcessed = await dbContext.ProcessedKafkaMessages.AnyAsync(
                    item => item.Topic == result.Topic
                        && item.Partition == result.Partition.Value
                        && item.Offset == result.Offset.Value,
                    stoppingToken);

                if (alreadyProcessed)
                {
                    consumer.Commit(result);
                    continue;
                }

                var integrationEvent = JsonSerializer.Deserialize<InvoiceIssuedIntegrationEvent>(result.Message.Value);
                if (integrationEvent is null)
                {
                    consumer.Commit(result);
                    continue;
                }

                var expeditionOrder = await repository.GetEntityByOrderIdAsync(integrationEvent.OrderId, stoppingToken);

                if (expeditionOrder is null)
                {
                    expeditionOrder = new ExpeditionOrder(
                        integrationEvent.OrderId,
                        integrationEvent.InvoiceId,
                        integrationEvent.CustomerId,
                        integrationEvent.Number,
                        integrationEvent.Series,
                        integrationEvent.AccessKey,
                        integrationEvent.IssuedAtUtc);

                    await repository.AddAsync(expeditionOrder, stoppingToken);
                    await eventPublisher.PublishStatusChangedAsync(expeditionOrder, stoppingToken);
                }

                await dbContext.ProcessedKafkaMessages.AddAsync(
                    new ProcessedKafkaMessage(result.Topic, result.Partition.Value, result.Offset.Value, groupId),
                    stoppingToken);

                await repository.SaveChangesAsync(stoppingToken);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while consuming invoice.issued.");
            }
        }
    }
}
