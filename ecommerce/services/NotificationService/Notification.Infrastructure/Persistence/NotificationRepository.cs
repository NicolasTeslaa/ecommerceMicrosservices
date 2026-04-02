using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<EmailNotification>> GetEmailNotificationsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailNotifications
            .Where(notification => notification.OrderId == orderId)
            .OrderBy(notification => notification.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<WhatsAppNotification>> GetWhatsAppNotificationsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WhatsAppNotifications
            .Where(notification => notification.OrderId == orderId)
            .OrderBy(notification => notification.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
