using Notification.Domain.Entities;

namespace Notification.Application.DTOs;

public class EmailNotificationDto
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public string SourceTopic { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int AttemptCount { get; init; }
    public string LastError { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? SentAtUtc { get; init; }

    public static EmailNotificationDto MapFromEntity(EmailNotification notification) =>
        new()
        {
            Id = notification.Id,
            OrderId = notification.OrderId,
            SourceTopic = notification.SourceTopic,
            EventType = notification.EventType,
            RecipientEmail = notification.RecipientEmail,
            Subject = notification.Subject,
            Body = notification.Body,
            Status = notification.Status.ToString(),
            AttemptCount = notification.AttemptCount,
            LastError = notification.LastError,
            CreatedAtUtc = notification.CreatedAtUtc,
            SentAtUtc = notification.SentAtUtc
        };
}
