using Confluent.Kafka;
using Expedition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Expedition.Infrastructure.Messaging;

public class ExpeditionOutboxDispatcherService : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExpeditionOutboxDispatcherService> _logger;

    public ExpeditionOutboxDispatcherService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<ExpeditionOutboxDispatcherService> logger)
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
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ExpeditionDbContext>();

                var pendingMessages = await dbContext.ExpeditionOutboxMessages
                    .Where(message => message.PublishedAtUtc == null)
                    .OrderBy(message => message.OccurredOnUtc)
                    .Take(BatchSize)
                    .ToListAsync(stoppingToken);

                if (pendingMessages.Count == 0)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                    continue;
                }

                foreach (var message in pendingMessages)
                {
                    var published = await TryPublishAsync(message, stoppingToken);

                    if (published)
                    {
                        message.MarkAsPublished();
                    }
                    else
                    {
                        message.RegisterPublishFailure("Background dispatch to Kafka failed.");
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while dispatching expedition outbox messages.");
                await Task.Delay(IdleDelay, stoppingToken);
            }
        }
    }

    private async Task<bool> TryPublishAsync(Expedition.Domain.Entities.ExpeditionOutboxMessage message, CancellationToken cancellationToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            _logger.LogWarning("Kafka:BootstrapServers was not configured for ExpeditionService outbox.");
            return false;
        }

        try
        {
            using var producer = new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            }).Build();

            await producer.ProduceAsync(
                message.Topic,
                new Message<string, string>
                {
                    Key = message.Key,
                    Value = message.Payload
                },
                cancellationToken);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to publish expedition outbox message '{OutboxMessageId}' to topic '{Topic}'.",
                message.Id,
                message.Topic);
            return false;
        }
    }
}
