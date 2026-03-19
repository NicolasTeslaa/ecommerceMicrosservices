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

    public async Task<Product?> FindEquivalentActiveAsync(
        string name,
        string description,
        decimal price,
        Guid categoryId,
        decimal heightCm,
        decimal widthCm,
        decimal cubageM3,
        decimal weightKg,
        string originZipCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedName = name.Trim();
            var normalizedDescription = description?.Trim() ?? string.Empty;
            var normalizedOriginZipCode = originZipCode.Trim();

            return await _context.Products.FirstOrDefaultAsync(
                product => product.Active
                    && product.Name == normalizedName
                    && product.Description == normalizedDescription
                    && product.Price == price
                    && product.CategoryId == categoryId
                    && product.HeightCm == heightCm
                    && product.WidthCm == widthCm
                    && product.CubageM3 == cubageM3
                    && product.WeightKg == weightKg
                    && product.OriginZipCode == normalizedOriginZipCode,
                cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException("Failed to search for an equivalent product.", exception);
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
