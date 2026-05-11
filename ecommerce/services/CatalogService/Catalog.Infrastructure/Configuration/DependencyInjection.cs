using Catalog.Application.Interfaces;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var writeConnectionString = GetConnectionStringOrFallback(configuration, "CatalogWriteDb");
        var readConnectionString = GetConnectionStringOrFallback(configuration, "CatalogReadDb");

        services.AddDbContext<CatalogWriteDbContext>(options =>
            options.UseMySql(
                writeConnectionString,
                new MySqlServerVersion(new Version(8, 0, 36))));

        services.AddDbContext<CatalogReadDbContext>(options =>
            options.UseMySql(
                readConnectionString,
                new MySqlServerVersion(new Version(8, 0, 36))));

        services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<IProductReadModelProjector, ProductReadModelProjector>();
        services.AddScoped<ICategoryWriteRepository, CategoryWriteRepository>();
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
        services.AddScoped<ICategoryReadModelProjector, CategoryReadModelProjector>();
        services.AddScoped<ICatalogProductIntegrationEventPublisher, Catalog.Infrastructure.Messaging.KafkaCatalogProductIntegrationEventPublisher>();

        return services;
    }

    private static string GetConnectionStringOrFallback(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            using var loggerFactory = LoggerFactory.Create(_ => { });
            loggerFactory.CreateLogger("Catalog.Infrastructure.Configuration.DependencyInjection")
                .LogError("Connection string '{ConnectionStringName}' was not configured. Using a fallback connection string.", connectionStringName);
            return $"Server=localhost;Port=3306;Database={connectionStringName.ToLowerInvariant()}_fallback;Uid=root;Pwd=root;";
        }

        return connectionString;
    }
}
