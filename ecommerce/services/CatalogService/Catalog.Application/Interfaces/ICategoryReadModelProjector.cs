using Catalog.Domain.Entities;

namespace Catalog.Application.Interfaces;

public interface ICategoryReadModelProjector
{
    Task UpsertAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
