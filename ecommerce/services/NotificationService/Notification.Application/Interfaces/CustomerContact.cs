namespace Notification.Application.Interfaces;

public sealed record CustomerContact(Guid CustomerId, string Email, string PhoneNumber);
