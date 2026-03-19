using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Order.Application.Interfaces;
using Order.Infrastructure.Persistence;

namespace Order.Infrastructure.Messaging;

public class OrderOutboxDispatcherService : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OrderOutboxDispatcherService> _logger;

    public OrderOutboxDispatcherService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OrderOutboxDispatcherService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
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
                var writeDbContext = scope.ServiceProvider.GetRequiredService<OrderWriteDbContext>();
                var queuePublisher = scope.ServiceProvider.GetRequiredService<IOrderProcessingQueuePublisher>();

                var pendingMessages = await writeDbContext.OrderProcessingOutboxMessages
                    .Where(message => message.DispatchedAtUtc == null)
                    .OrderBy(message => message.RequestedAtUtc)
                    .Take(BatchSize)
                    .ToListAsync(stoppingToken);

                if (pendingMessages.Count == 0)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                    continue;
                }

                foreach (var message in pendingMessages)
                {
                    message.MarkDispatchAttempt();
                    var published = await queuePublisher.TryPublishAsync(message.Id, stoppingToken);

                    if (published)
                        message.MarkAsDispatched();
                    else
                        message.RegisterDispatchFailure("Background dispatch to order.processing.requested failed.");

                    await writeDbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while dispatching order outbox messages.");
                await Task.Delay(IdleDelay, stoppingToken);
            }
        }
    }
}
