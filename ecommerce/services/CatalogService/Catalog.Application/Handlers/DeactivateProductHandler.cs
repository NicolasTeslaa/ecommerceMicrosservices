using Catalog.Application.Commands;
using Catalog.Application.Interfaces;
using MediatR;

namespace Catalog.Application.Handlers;

public class DeactivateProductHandler : IRequestHandler<DeactivateProductCommand, Guid>
{
    private readonly IProductWriteRepository _repository;
    private readonly IProductReadModelProjector _projector;
    private readonly ICatalogProductIntegrationEventPublisher _integrationEventPublisher;

    public DeactivateProductHandler(
        IProductWriteRepository repository,
        IProductReadModelProjector projector,
        ICatalogProductIntegrationEventPublisher integrationEventPublisher)
    {
        _repository = repository;
        _projector = projector;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task<Guid> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            return Guid.Empty;

        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return Guid.Empty;

        product.Deactivate();

        await _repository.UpdateAsync(product, cancellationToken);
        await _projector.UpsertAsync(product, cancellationToken);
        await _integrationEventPublisher.PublishProductCreatedAsync(product, 0, cancellationToken);

        return product.Id;
    }
}
