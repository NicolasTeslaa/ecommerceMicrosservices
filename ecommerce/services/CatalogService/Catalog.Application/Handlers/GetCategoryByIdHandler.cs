using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Application.Queries;
using Catalog.Domain.Exceptions;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoryByIdHandler(ICategoryReadRepository repository) => _repository = repository;

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new InvalidCategoryIdException();

        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
            throw new CategoryNotFoundException(request.Id);

        return CategoryDto.MapFromReadModel(category);
    }
}
