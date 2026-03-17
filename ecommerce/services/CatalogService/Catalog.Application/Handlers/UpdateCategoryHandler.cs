using Catalog.Application.Commands;
using Catalog.Application.Interfaces;
using Catalog.Domain.Exceptions;
using MediatR;

namespace Catalog.Application.Handlers;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _repository;

    public UpdateCategoryHandler(ICategoryRepository repository) => _repository = repository;

    public async Task<Guid> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new InvalidCategoryIdException();

        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
            throw new CategoryNotFoundException(request.Id);

        category.Update(request.Name);

        await _repository.UpdateAsync(category, cancellationToken);

        return category.Id;
    }
}
