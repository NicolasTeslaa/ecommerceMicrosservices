using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Messaging;

public class NotificationDispatchService : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private const int BatchSize = 25;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<NotificationDispatchService> _logger;

    public NotificationDispatchService(IServiceScopeFactory serviceScopeFactory, ILogger<NotificationDispatchService> logger)
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
                var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

                var pendingEmails = await dbContext.EmailNotifications
                    .Where(notification => notification.Status == NotificationDeliveryStatus.Pending)
                    .OrderBy(notification => notification.CreatedAtUtc)
                    .Take(BatchSize)
                    .ToListAsync(stoppingToken);

                foreach (var notification in pendingEmails)
                {
                    notification.MarkAsSent();
                    _logger.LogInformation(
                        "Simulated email sent for order '{OrderId}' to '{RecipientEmail}' from topic '{Topic}'.",
                        notification.OrderId,
                        notification.RecipientEmail,
                        notification.SourceTopic);
                }

                var pendingWhatsAppMessages = await dbContext.WhatsAppNotifications
                    .Where(notification => notification.Status == NotificationDeliveryStatus.Pending)
                    .OrderBy(notification => notification.CreatedAtUtc)
                    .Take(BatchSize)
                    .ToListAsync(stoppingToken);

                foreach (var notification in pendingWhatsAppMessages)
                {
                    notification.MarkAsSent();
                    _logger.LogInformation(
                        "Simulated WhatsApp notification sent for order '{OrderId}' to '{RecipientPhoneNumber}' from topic '{Topic}'.",
                        notification.OrderId,
                        notification.RecipientPhoneNumber,
                        notification.SourceTopic);
                }

                if (pendingEmails.Count == 0 && pendingWhatsAppMessages.Count == 0)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                    continue;
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while dispatching notification messages.");
                await Task.Delay(IdleDelay, stoppingToken);
            }
        }
    }
}
