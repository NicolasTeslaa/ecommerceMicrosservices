using ECommerce.Shared.Messaging;

namespace Auth.Application.Interfaces;

public interface IAuthEventPublisher
{
    Task PublishUserRegisteredAsync(UserRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
