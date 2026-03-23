using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Messaging;

public class PaymentOutboxDispatcherService : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentOutboxDispatcherService> _logger;

    public PaymentOutboxDispatcherService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<PaymentOutboxDispatcherService> logger)
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
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                var pendingMessages = await dbContext.PaymentOutboxMessages
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
                _logger.LogError(exception, "Unexpected error while dispatching payment outbox messages.");
                await Task.Delay(IdleDelay, stoppingToken);
            }
        }
    }

    private async Task<bool> TryPublishAsync(Payment.Domain.Entities.PaymentOutboxMessage message, CancellationToken cancellationToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            _logger.LogWarning("Kafka:BootstrapServers was not configured for PaymentService outbox.");
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
                    Key = string.IsNullOrWhiteSpace(message.Key) ? message.PaymentId.ToString() : message.Key,
                    Value = message.Payload
                },
                cancellationToken);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to publish payment outbox message '{OutboxMessageId}' to topic '{Topic}'.",
                message.Id,
                message.Topic);
            return false;
        }
    }
}
