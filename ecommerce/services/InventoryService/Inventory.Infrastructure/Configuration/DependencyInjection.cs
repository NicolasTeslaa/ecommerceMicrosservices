using Inventory.Application.Interfaces;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Synchronization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetConnectionStringOrFallback(configuration, "InventoryDb");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInventoryEventPublisher, KafkaInventoryEventPublisher>();
        services.AddScoped<PaymentApprovedMessageProcessor>();
        services.AddScoped<PaymentFailedMessageProcessor>();
        services.AddHttpClient("catalog-read", client =>
        {
            client.BaseAddress = new Uri(configuration["CatalogService:BaseUrl"] ?? "https://localhost:5101");
        });
        services.AddHostedService<CatalogInventoryBootstrapService>();
        services.AddHostedService<CatalogProductCreatedConsumerService>();
        services.AddHostedService<PaymentApprovedConsumerService>();
        services.AddHostedService<PaymentFailedConsumerService>();

        return services;
    }

    private static string GetConnectionStringOrFallback(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            using var loggerFactory = LoggerFactory.Create(_ => { });
            loggerFactory.CreateLogger("Inventory.Infrastructure.Configuration.DependencyInjection")
                .LogError("Connection string '{ConnectionStringName}' was not configured. Using a fallback connection string.", connectionStringName);
            return $"Server=localhost;Port=3306;Database={connectionStringName.ToLowerInvariant()}_fallback;Uid=root;Pwd=root;";
        }

        return connectionString;
    }
}
