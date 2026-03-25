using Inventory.Application.Interfaces;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Synchronization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InventoryDb")
            ?? throw new InvalidOperationException("Connection string 'InventoryDb' was not configured.");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInventoryEventPublisher, KafkaInventoryEventPublisher>();
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
}
