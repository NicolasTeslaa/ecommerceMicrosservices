using Catalog.Domain.Entities;

namespace Catalog.Application.Interfaces;

public interface IProductReadModelProjector
{
    Task UpsertAsync(Product product, CancellationToken cancellationToken = default);
}
