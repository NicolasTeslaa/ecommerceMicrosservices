using Catalog.Application.Interfaces;
using Catalog.Application.ReadModels;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Persistence;

public class CategoryReadModelProjector : ICategoryReadModelProjector
{
    private readonly CatalogReadDbContext _context;
    private readonly ILogger<CategoryReadModelProjector> _logger;

    public CategoryReadModelProjector(CatalogReadDbContext context, ILogger<CategoryReadModelProjector> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task UpsertAsync(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(current => current.Id == category.Id, cancellationToken);

            if (existingCategory is null)
            {
                await _context.Categories.AddAsync(new CategoryReadModel
                {
                    Id = category.Id,
                    Name = category.Name
                }, cancellationToken);
            }
            else
            {
                existingCategory.Name = category.Name;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to project category {CategoryId} to the read database. The write database remains the source of truth.",
                category.Id);
        }
    }

    public async Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(current => current.Id == categoryId, cancellationToken);

            if (existingCategory is null)
                return;

            _context.Categories.Remove(existingCategory);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to remove category {CategoryId} from the read database. The write database remains the source of truth.",
                categoryId);
        }
    }
}
