using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Application.Queries;
using Catalog.Domain.Exceptions;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductReadRepository _repository;

    public GetProductByIdHandler(IProductReadRepository repository) => _repository = repository;

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new InvalidProductIdException();

        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(request.Id);

        return ProductDto.MapFromReadModel(product);
    }
}
