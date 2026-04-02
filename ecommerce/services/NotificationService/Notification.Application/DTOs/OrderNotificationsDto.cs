namespace Notification.Application.DTOs;

public class OrderNotificationsDto
{
    public Guid OrderId { get; init; }
    public IReadOnlyCollection<EmailNotificationDto> Emails { get; init; } = Array.Empty<EmailNotificationDto>();
    public IReadOnlyCollection<WhatsAppNotificationDto> WhatsAppMessages { get; init; } = Array.Empty<WhatsAppNotificationDto>();
}
