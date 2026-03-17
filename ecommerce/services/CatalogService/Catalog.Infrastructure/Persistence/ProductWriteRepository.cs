using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public class ProductWriteRepository : IProductWriteRepository
{
    private readonly CatalogWriteDbContext _context;

    public ProductWriteRepository(CatalogWriteDbContext context) => _context = context;

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException("Failed to persist the product.", exception);
        }
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Products
                .FirstOrDefaultAsync(product => product.Id == id && product.Active, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException($"Failed to retrieve product '{id}'.", exception);
        }
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException($"Failed to update product '{product.Id}'.", exception);
        }
    }
}
