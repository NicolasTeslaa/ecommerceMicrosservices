using Catalog.Domain.Entities;

namespace Catalog.Application.Interfaces;

public interface ICatalogProductIntegrationEventPublisher
{
    Task PublishProductCreatedAsync(
        Product product,
        int stockDelta,
        CancellationToken cancellationToken = default);
}
