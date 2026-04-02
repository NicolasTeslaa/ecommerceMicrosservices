using Notification.Domain.Entities;

namespace Notification.Application.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyCollection<EmailNotification>> GetEmailNotificationsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<WhatsAppNotification>> GetWhatsAppNotificationsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
