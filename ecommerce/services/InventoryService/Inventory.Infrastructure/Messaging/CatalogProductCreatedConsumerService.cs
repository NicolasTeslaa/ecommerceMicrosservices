using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.Messaging;

public class CatalogProductCreatedConsumerService : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CatalogProductCreatedConsumerService> _logger;

    public CatalogProductCreatedConsumerService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<CatalogProductCreatedConsumerService> logger)
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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var bootstrapServers = _configuration["Kafka:BootstrapServers"];
                if (string.IsNullOrWhiteSpace(bootstrapServers))
                {
                    _logger.LogWarning("Inventory catalog-product consumer is waiting because Kafka:BootstrapServers was not configured.");
                    await Task.Delay(RetryDelay, stoppingToken);
                    continue;
                }

                var topic = _configuration["Kafka:CatalogProductCreatedTopic"] ?? "catalog.product-created";
                var groupId = _configuration["Kafka:CatalogProductCreatedConsumerGroup"] ?? "inventory-catalog-products";

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
                    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                    var repository = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();

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

                    var integrationEvent = JsonSerializer.Deserialize<CatalogProductCreatedIntegrationEvent>(result.Message.Value);
                    if (integrationEvent is null)
                    {
                        _logger.LogWarning("Inventory catalog-product consumer ignored a message because it could not deserialize CatalogProductCreatedIntegrationEvent.");
                        consumer.Commit(result);
                        continue;
                    }

                    var existingItem = await repository.GetItemByProductIdAsync(integrationEvent.ProductId, stoppingToken);

                    if (existingItem is null)
                    {
                        await repository.AddItemAsync(
                            new InventoryItem(
                                integrationEvent.ProductId,
                                integrationEvent.ProductName,
                                integrationEvent.InitialStockQuantity,
                                integrationEvent.Active),
                            stoppingToken);
                    }
                    else
                    {
                        existingItem.UpdateCatalogMetadata(integrationEvent.ProductName, integrationEvent.Active);

                        if (integrationEvent.InitialStockQuantity > 0)
                            existingItem.IncreaseStock(integrationEvent.InitialStockQuantity);
                    }

                    await dbContext.ProcessedKafkaMessages.AddAsync(
                        new ProcessedKafkaMessage(result.Topic, result.Partition.Value, result.Offset.Value, groupId),
                        stoppingToken);
                    await repository.SaveChangesAsync(stoppingToken);
                    consumer.Commit(result);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while consuming catalog.product-created.");
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }
}
