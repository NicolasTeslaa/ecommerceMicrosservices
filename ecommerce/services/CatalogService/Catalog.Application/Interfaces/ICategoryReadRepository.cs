using Catalog.Application.ReadModels;
using ECommerce.Shared.Contracts;

namespace Catalog.Application.Interfaces;

public interface ICategoryReadRepository
{
    Task<CategoryReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<CategoryReadModel>> GetAllAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
}
