using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Application.Queries;
using ECommerce.Shared.Contracts;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductReadRepository _repository;

    public GetAllProductsHandler(IProductReadRepository repository) => _repository = repository;

    public async Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(request, cancellationToken);

        return products.Map(ProductDto.MapFromReadModel);
    }
}
