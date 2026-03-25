using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.Messaging;

public class PaymentApprovedConsumerService : BackgroundService
{
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
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(bootstrapServers))
            throw new InvalidOperationException("Kafka:BootstrapServers was not configured for InventoryService.");

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

                var integrationEvent = JsonSerializer.Deserialize<PaymentApprovedIntegrationEvent>(result.Message.Value);
                if (integrationEvent is null)
                    continue;

                var reservations = await repository.GetReservationsByOrderIdAsync(integrationEvent.OrderId, stoppingToken);
                var items = await repository.GetItemsByProductIdsAsync(reservations.Select(item => item.ProductId).Distinct().ToArray(), stoppingToken);
                var itemsByProductId = items.ToDictionary(item => item.ProductId);

                foreach (var reservation in reservations.Where(item => item.Status == InventoryReservationStatus.Pending))
                {
                    if (!itemsByProductId.TryGetValue(reservation.ProductId, out var inventoryItem))
                        continue;

                    inventoryItem.ConfirmReservation(reservation.Quantity);
                    reservation.Confirm();
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
                _logger.LogError(exception, "Unexpected error while consuming payment.approved.");
            }
        }
    }
}
