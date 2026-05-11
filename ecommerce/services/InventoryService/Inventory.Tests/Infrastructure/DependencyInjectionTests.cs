using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Inventory.Application.Interfaces;
using Inventory.Infrastructure.Configuration;
using Inventory.Infrastructure.Messaging;

namespace Inventory.Tests.Infrastructure;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterCoreServices_WhenConnectionStringIsMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddInfrastructure(configuration);

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IInventoryRepository));
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterCoreServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:InventoryDb"] = "server=localhost;database=test;user=root;password=123",
                ["CatalogService:BaseUrl"] = "https://localhost:5101"
            })
            .Build();

        services.AddInfrastructure(configuration);

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IInventoryRepository));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IInventoryEventPublisher));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(PaymentApprovedMessageProcessor));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(PaymentFailedMessageProcessor));
        Assert.Contains(services, descriptor => descriptor.ImplementationType?.Name == "CatalogInventoryBootstrapService");
        Assert.Contains(services, descriptor => descriptor.ImplementationType?.Name == "CatalogProductCreatedConsumerService");
        Assert.Contains(services, descriptor => descriptor.ImplementationType?.Name == "PaymentApprovedConsumerService");
        Assert.Contains(services, descriptor => descriptor.ImplementationType?.Name == "PaymentFailedConsumerService");
    }
}
