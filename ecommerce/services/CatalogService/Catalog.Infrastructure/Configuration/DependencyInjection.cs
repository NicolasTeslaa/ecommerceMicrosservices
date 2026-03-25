using Catalog.Application.Interfaces;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var writeConnectionString = GetRequiredConnectionString(configuration, "CatalogWriteDb");
        var readConnectionString = GetRequiredConnectionString(configuration, "CatalogReadDb");

        services.AddDbContext<CatalogWriteDbContext>(options =>
            options.UseMySql(
                writeConnectionString,
                ServerVersion.AutoDetect(writeConnectionString)));

        services.AddDbContext<CatalogReadDbContext>(options =>
            options.UseMySql(
                readConnectionString,
                ServerVersion.AutoDetect(readConnectionString)));

        services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<IProductReadModelProjector, ProductReadModelProjector>();
        services.AddScoped<ICategoryWriteRepository, CategoryWriteRepository>();
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
        services.AddScoped<ICategoryReadModelProjector, CategoryReadModelProjector>();
        services.AddScoped<ICatalogProductIntegrationEventPublisher, Catalog.Infrastructure.Messaging.KafkaCatalogProductIntegrationEventPublisher>();

        return services;
    }

    private static string GetRequiredConnectionString(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' was not configured.");

        return connectionString;
    }
}
