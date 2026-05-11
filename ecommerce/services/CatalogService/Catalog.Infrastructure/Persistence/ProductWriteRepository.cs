using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catalog.Infrastructure.Persistence;

public class ProductWriteRepository : IProductWriteRepository
{
    private readonly CatalogWriteDbContext _context;
    private readonly ILogger<ProductWriteRepository> _logger;

    public ProductWriteRepository(CatalogWriteDbContext context, ILogger<ProductWriteRepository>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<ProductWriteRepository>.Instance;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to persist product '{ProductId}'.", product.Id);
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
            _logger.LogError(exception, "Failed to search for an equivalent product.");
            return null;
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
            _logger.LogError(exception, "Failed to retrieve product '{ProductId}'.", id);
            return null;
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
            _logger.LogError(exception, "Failed to update product '{ProductId}'.", product.Id);
        }
    }
}
