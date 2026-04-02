namespace ECommerce.Shared.Messaging;

public class UserRegisteredIntegrationEvent
{
    public Guid AuthUserId { get; init; }
    public Guid CustomerId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime RegisteredAtUtc { get; init; }
}
