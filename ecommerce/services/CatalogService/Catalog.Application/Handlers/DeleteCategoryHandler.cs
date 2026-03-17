using Catalog.Application.Commands;
using Catalog.Application.Interfaces;
using Catalog.Domain.Exceptions;
using MediatR;

namespace Catalog.Application.Handlers;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, Guid>
{
    private readonly ICategoryRepository _repository;

    public DeleteCategoryHandler(ICategoryRepository repository) => _repository = repository;

    public async Task<Guid> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new InvalidCategoryIdException();

        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
            throw new CategoryNotFoundException(request.Id);

        await _repository.DeleteAsync(category, cancellationToken);

        return category.Id;
    }
}
