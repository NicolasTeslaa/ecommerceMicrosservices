using Catalog.Application.Interfaces;
using Catalog.Application.ReadModels;
using ECommerce.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catalog.Infrastructure.Persistence;

public class CategoryReadRepository : ICategoryReadRepository
{
    private readonly CatalogReadDbContext _context;
    private readonly ILogger<CategoryReadRepository> _logger;

    public CategoryReadRepository(CatalogReadDbContext context, ILogger<CategoryReadRepository>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<CategoryReadRepository>.Instance;
    }

    public async Task<PagedResult<CategoryReadModel>> GetAllAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .ThenBy(category => category.Id);

            var totalItems = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return PagedResult<CategoryReadModel>.Create(items, pagination.PageNumber, pagination.PageSize, totalItems);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve categories from the read database.");
            return PagedResult<CategoryReadModel>.Create(Array.Empty<CategoryReadModel>(), pagination.PageNumber, pagination.PageSize, 0);
        }
    }

    public async Task<CategoryReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve category '{CategoryId}' from the read database.", id);
            return null;
        }
    }
}
