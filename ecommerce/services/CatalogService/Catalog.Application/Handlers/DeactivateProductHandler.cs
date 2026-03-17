using Catalog.Application.Commands;
using Catalog.Application.Interfaces;
using Catalog.Domain.Exceptions;
using MediatR;

namespace Catalog.Application.Handlers;

public class DeactivateProductHandler : IRequestHandler<DeactivateProductCommand, Guid>
{
    private readonly IProductRepository _repository;

    public DeactivateProductHandler(IProductRepository repository) => _repository = repository;

    public async Task<Guid> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new InvalidProductIdException();

        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(request.Id);

        product.Deactivate();

        await _repository.UpdateAsync(product, cancellationToken);

        return product.Id;
    }
}
