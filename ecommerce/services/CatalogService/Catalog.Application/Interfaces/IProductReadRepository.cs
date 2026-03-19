using Catalog.Application.ReadModels;
using Catalog.Application.Queries;
using ECommerce.Shared.Contracts;

namespace Catalog.Application.Interfaces;

public interface IProductReadRepository
{
    Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductReadModel>> GetAllAsync(GetAllProductsQuery query, CancellationToken cancellationToken = default);
}
