using Catalog.Application.Interfaces;
using Catalog.Application.ReadModels;
using Catalog.Domain.Exceptions;
using ECommerce.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public class ProductReadRepository : IProductReadRepository
{
    private readonly CatalogReadDbContext _context;

    public ProductReadRepository(CatalogReadDbContext context) => _context = context;

    public async Task<PagedResult<ProductReadModel>> GetAllAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Products
                .Where(product => product.Active)
                .AsNoTracking()
                .OrderBy(product => product.Name)
                .ThenBy(product => product.Id);

            var totalItems = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return PagedResult<ProductReadModel>.Create(items, pagination.PageNumber, pagination.PageSize, totalItems);
        }
        catch (Exception exception)
        {
            throw new PersistenceException("Failed to retrieve products from the read database.", exception);
        }
    }

    public async Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Products
                .Where(product => product.Active)
                .AsNoTracking()
                .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException($"Failed to retrieve product '{id}' from the read database.", exception);
        }
    }
}
