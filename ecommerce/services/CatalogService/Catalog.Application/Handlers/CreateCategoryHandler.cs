using Catalog.Application.Commands;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Handlers;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryWriteRepository _repository;
    private readonly ICategoryReadModelProjector _projector;

    public CreateCategoryHandler(ICategoryWriteRepository repository, ICategoryReadModelProjector projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category(request.Name);

        await _repository.AddAsync(category, cancellationToken);
        await _projector.UpsertAsync(category, cancellationToken);

        return category.Id;
    }
}
