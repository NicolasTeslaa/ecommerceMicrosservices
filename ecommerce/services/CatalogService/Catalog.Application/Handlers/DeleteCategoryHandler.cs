using Catalog.Application.Commands;
using Catalog.Application.Interfaces;
using MediatR;

namespace Catalog.Application.Handlers;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, Guid>
{
    private readonly ICategoryWriteRepository _repository;
    private readonly ICategoryReadModelProjector _projector;

    public DeleteCategoryHandler(ICategoryWriteRepository repository, ICategoryReadModelProjector projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async Task<Guid> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            return Guid.Empty;

        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
            return Guid.Empty;

        await _repository.DeleteAsync(category, cancellationToken);
        await _projector.DeleteAsync(category.Id, cancellationToken);

        return category.Id;
    }
}
