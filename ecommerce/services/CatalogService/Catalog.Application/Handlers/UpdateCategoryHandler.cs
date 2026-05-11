using Catalog.Application.Commands;
using Catalog.Application.Interfaces;
using MediatR;

namespace Catalog.Application.Handlers;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Guid>
{
    private readonly ICategoryWriteRepository _repository;
    private readonly ICategoryReadModelProjector _projector;

    public UpdateCategoryHandler(ICategoryWriteRepository repository, ICategoryReadModelProjector projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async Task<Guid> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            return Guid.Empty;

        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
            return Guid.Empty;

        category.Update(request.Name);

        await _repository.UpdateAsync(category, cancellationToken);
        await _projector.UpsertAsync(category, cancellationToken);

        return category.Id;
    }
}
