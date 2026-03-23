using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Shared.Protos;
using Order.Application.Interfaces;
using Order.Infrastructure.Clients;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Persistence;

namespace Order.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool enableOutboxDispatcher = false,
        bool enableProcessorConsumer = false)
    {
        var writeConnectionString = GetRequiredConnectionString(configuration, "OrderWriteDb");
        var readConnectionString = GetRequiredConnectionString(configuration, "OrderReadDb");

        services.AddDbContext<OrderWriteDbContext>(options =>
            options.UseMySql(
                writeConnectionString,
                ServerVersion.AutoDetect(writeConnectionString)));

        services.AddDbContext<OrderReadDbContext>(options =>
            options.UseMySql(
                readConnectionString,
                ServerVersion.AutoDetect(readConnectionString)));

        services.AddScoped<IOrderWriteRepository, OrderWriteRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();
        services.AddScoped<IOrderReadModelProjector, OrderReadModelProjector>();
        services.AddScoped<IOrderEventPublisher, KafkaOrderEventPublisher>();
        services.AddScoped<IOrderCheckoutService, OrderCheckoutService>();
        services.AddScoped<IOrderProcessingQueuePublisher, KafkaOrderProcessingQueuePublisher>();
        services.AddScoped<ICustomerAddressValidationClient, CustomerAddressValidationGrpcClient>();
        services.AddScoped<ICatalogProductAvailabilityClient, CatalogProductAvailabilityGrpcClient>();

        if (enableOutboxDispatcher)
            services.AddHostedService<OrderOutboxDispatcherService>();

        if (enableProcessorConsumer)
        {
            services.AddHostedService<OrderProcessorConsumerService>();
            services.AddHostedService<PaymentResultConsumerService>();
        }

        services.AddGrpcClient<CustomerAddressValidation.CustomerAddressValidationClient>(options =>
        {
            options.Address = new Uri(configuration["CustomerService:BaseUrl"] ?? "https://localhost:5107");
        });

        services.AddGrpcClient<CatalogProductAvailability.CatalogProductAvailabilityClient>(options =>
        {
            options.Address = new Uri(configuration["CatalogService:BaseUrl"] ?? "https://localhost:5101");
        });

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
