using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Messaging;

public class AuthOutboxPublisherService : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthOutboxPublisherService> _logger;

    public AuthOutboxPublisherService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<AuthOutboxPublisherService> logger)
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
            throw new InvalidOperationException("Kafka:BootstrapServers was not configured for AuthService.");

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

                var pendingMessages = await dbContext.OutboxMessages
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
                    await PublishMessageAsync(producer, dbContext, message, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while publishing AuthService outbox messages.");
                await Task.Delay(IdleDelay, stoppingToken);
            }
        }
    }

    private async Task PublishMessageAsync(
        IProducer<string, string> producer,
        AuthDbContext dbContext,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await producer.ProduceAsync(
                message.Topic,
                new Message<string, string>
                {
                    Key = message.Key,
                    Value = message.Payload
                },
                cancellationToken);

            message.MarkAsPublished();
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Auth outbox message '{OutboxMessageId}' published to topic '{Topic}'.",
                message.Id,
                message.Topic);
        }
        catch (Exception exception)
        {
            message.RegisterPublishFailure(exception.Message);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                exception,
                "Failed to publish Auth outbox message '{OutboxMessageId}' to topic '{Topic}'. Attempt {Attempt}.",
                message.Id,
                message.Topic,
                message.PublishAttempts);
        }
    }
}
