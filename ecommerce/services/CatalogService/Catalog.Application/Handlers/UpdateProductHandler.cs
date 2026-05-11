using Catalog.Application.Commands;
using Catalog.Application.Interfaces;
using MediatR;

namespace Catalog.Application.Handlers;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Guid>
{
    private readonly IProductWriteRepository _repository;
    private readonly IProductReadModelProjector _projector;
    private readonly ICategoryWriteRepository _categoryRepository;
    private readonly ICatalogProductIntegrationEventPublisher _integrationEventPublisher;

    public UpdateProductHandler(
        IProductWriteRepository repository,
        IProductReadModelProjector projector,
        ICategoryWriteRepository categoryRepository,
        ICatalogProductIntegrationEventPublisher integrationEventPublisher)
    {
        _repository = repository;
        _projector = projector;
        _categoryRepository = categoryRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task<Guid> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            return Guid.Empty;

        if (request.CategoryId == Guid.Empty)
            return Guid.Empty;

        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return Guid.Empty;

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
            return Guid.Empty;

        product.Update(
            request.Name,
            request.Description,
            request.Price,
            request.CategoryId,
            request.HeightCm,
            request.WidthCm,
            request.CubageM3,
            request.WeightKg,
            request.OriginZipCode);

        await _repository.UpdateAsync(product, cancellationToken);
        await _projector.UpsertAsync(product, cancellationToken);
        await _integrationEventPublisher.PublishProductCreatedAsync(product, 0, cancellationToken);

        return product.Id;
    }
}
