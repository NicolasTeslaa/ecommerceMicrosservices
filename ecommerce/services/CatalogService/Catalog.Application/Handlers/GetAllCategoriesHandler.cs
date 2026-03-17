using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Application.Queries;
using ECommerce.Shared.Contracts;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, PagedResult<CategoryDto>>
{
    private readonly ICategoryReadRepository _repository;

    public GetAllCategoriesHandler(ICategoryReadRepository repository) => _repository = repository;

    public async Task<PagedResult<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _repository.GetAllAsync(request, cancellationToken);

        return categories.Map(CategoryDto.MapFromReadModel);
    }
}
