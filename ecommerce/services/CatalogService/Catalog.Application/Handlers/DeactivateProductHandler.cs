using Catalog.Application.Commands;
using Catalog.Application.Interfaces;
using Catalog.Domain.Exceptions;
using MediatR;

namespace Catalog.Application.Handlers;

public class DeactivateProductHandler : IRequestHandler<DeactivateProductCommand, Guid>
{
    private readonly IProductWriteRepository _repository;
    private readonly IProductReadModelProjector _projector;

    public DeactivateProductHandler(IProductWriteRepository repository, IProductReadModelProjector projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async Task<Guid> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new InvalidProductIdException();

        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(request.Id);

        product.Deactivate();

        await _repository.UpdateAsync(product, cancellationToken);
        await _projector.UpsertAsync(product, cancellationToken);

        return product.Id;
    }
}
