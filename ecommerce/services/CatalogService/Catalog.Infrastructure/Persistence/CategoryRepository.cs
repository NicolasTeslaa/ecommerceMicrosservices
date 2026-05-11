using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catalog.Infrastructure.Persistence;

public class CategoryWriteRepository : ICategoryWriteRepository
{
    private readonly CatalogWriteDbContext _context;
    private readonly ILogger<CategoryWriteRepository> _logger;

    public CategoryWriteRepository(CatalogWriteDbContext context, ILogger<CategoryWriteRepository>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<CategoryWriteRepository>.Instance;
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Categories.AddAsync(category, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to persist category '{CategoryId}'.", category.Id);
        }
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Categories
                .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve category '{CategoryId}'.", id);
            return null;
        }
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to update category '{CategoryId}'.", category.Id);
        }
    }

    public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to delete category '{CategoryId}'.", category.Id);
        }
    }
}
