using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Application.Queries;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoryByIdHandler(ICategoryReadRepository repository) => _repository = repository;

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            return new CategoryDto { Id = Guid.Empty };

        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
            return new CategoryDto { Id = request.Id };

        return CategoryDto.MapFromReadModel(category);
    }
}
