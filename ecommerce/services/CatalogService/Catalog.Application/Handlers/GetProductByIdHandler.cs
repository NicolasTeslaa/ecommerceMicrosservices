using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Application.Queries;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductReadRepository _repository;

    public GetProductByIdHandler(IProductReadRepository repository) => _repository = repository;

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            return new ProductDto { Id = Guid.Empty };

        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return new ProductDto { Id = request.Id };

        return ProductDto.MapFromReadModel(product);
    }
}
