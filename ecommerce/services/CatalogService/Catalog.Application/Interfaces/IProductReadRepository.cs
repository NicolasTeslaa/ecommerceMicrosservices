using Catalog.Application.ReadModels;
using ECommerce.Shared.Contracts;

namespace Catalog.Application.Interfaces;

public interface IProductReadRepository
{
    Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductReadModel>> GetAllAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
}
