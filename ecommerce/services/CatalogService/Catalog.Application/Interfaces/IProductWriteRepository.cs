using Catalog.Domain.Entities;

namespace Catalog.Application.Interfaces;

public interface IProductWriteRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> FindEquivalentActiveAsync(
        string name,
        string description,
        decimal price,
        Guid categoryId,
        decimal heightCm,
        decimal widthCm,
        decimal cubageM3,
        decimal weightKg,
        string originZipCode,
        CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
}
