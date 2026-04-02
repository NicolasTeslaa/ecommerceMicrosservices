using Notification.Domain.Entities;

namespace Notification.Application.DTOs;

public class WhatsAppNotificationDto
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public string SourceTopic { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string RecipientPhoneNumber { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int AttemptCount { get; init; }
    public string LastError { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? SentAtUtc { get; init; }

    public static WhatsAppNotificationDto MapFromEntity(WhatsAppNotification notification) =>
        new()
        {
            Id = notification.Id,
            OrderId = notification.OrderId,
            SourceTopic = notification.SourceTopic,
            EventType = notification.EventType,
            RecipientPhoneNumber = notification.RecipientPhoneNumber,
            Message = notification.Message,
            Status = notification.Status.ToString(),
            AttemptCount = notification.AttemptCount,
            LastError = notification.LastError,
            CreatedAtUtc = notification.CreatedAtUtc,
            SentAtUtc = notification.SentAtUtc
        };
}
