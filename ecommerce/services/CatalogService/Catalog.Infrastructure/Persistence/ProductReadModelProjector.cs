using Catalog.Application.Interfaces;
using Catalog.Application.ReadModels;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Persistence;

public class ProductReadModelProjector : IProductReadModelProjector
{
    private readonly CatalogReadDbContext _context;
    private readonly ILogger<ProductReadModelProjector> _logger;

    public ProductReadModelProjector(CatalogReadDbContext context, ILogger<ProductReadModelProjector> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task UpsertAsync(Product product, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(current => current.Id == product.Id, cancellationToken);

            if (existingProduct is null)
            {
                await _context.Products.AddAsync(new ProductReadModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    Active = product.Active,
                    CategoryId = product.CategoryId,
                    HeightCm = product.HeightCm,
                    WidthCm = product.WidthCm,
                    CubageM3 = product.CubageM3,
                    WeightKg = product.WeightKg,
                    OriginZipCode = product.OriginZipCode
                }, cancellationToken);
            }
            else
            {
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.Active = product.Active;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.HeightCm = product.HeightCm;
                existingProduct.WidthCm = product.WidthCm;
                existingProduct.CubageM3 = product.CubageM3;
                existingProduct.WeightKg = product.WeightKg;
                existingProduct.OriginZipCode = product.OriginZipCode;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to project product {ProductId} to the read database. The write database remains the source of truth.",
                product.Id);
        }
    }
}
