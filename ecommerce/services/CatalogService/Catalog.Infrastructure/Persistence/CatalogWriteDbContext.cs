using Catalog.Domain.Entities;
using Catalog.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public class CatalogWriteDbContext : DbContext
{
    public CatalogWriteDbContext(DbContextOptions<CatalogWriteDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
