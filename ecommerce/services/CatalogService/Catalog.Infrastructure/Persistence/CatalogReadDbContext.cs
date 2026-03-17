using Catalog.Application.ReadModels;
using Catalog.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public class CatalogReadDbContext : DbContext
{
    public CatalogReadDbContext(DbContextOptions<CatalogReadDbContext> options) : base(options)
    {
    }

    public DbSet<ProductReadModel> Products => Set<ProductReadModel>();
    public DbSet<CategoryReadModel> Categories => Set<CategoryReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductReadModelConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryReadModelConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
