using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        var writeConnectionString = GetConnectionStringOrFallback(configuration, "OrderWriteDb");
        var readConnectionString = GetConnectionStringOrFallback(configuration, "OrderReadDb");

        services.AddDbContext<OrderWriteDbContext>(options =>
            options.UseMySql(
                writeConnectionString,
                new MySqlServerVersion(new Version(8, 0, 36))));

        services.AddDbContext<OrderReadDbContext>(options =>
            options.UseMySql(
                readConnectionString,
                new MySqlServerVersion(new Version(8, 0, 36))));

        services.AddScoped<IOrderWriteRepository, OrderWriteRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();
        services.AddScoped<IOrderReadModelProjector, OrderReadModelProjector>();
        services.AddScoped<IOrderEventPublisher, KafkaOrderEventPublisher>();
        services.AddScoped<IOrderCheckoutService, OrderCheckoutService>();
        services.AddScoped<IOrderCancellationService, OrderCancellationService>();
        services.AddScoped<IOrderProcessingQueuePublisher, KafkaOrderProcessingQueuePublisher>();
        services.AddScoped<ICustomerAddressValidationClient, CustomerAddressValidationGrpcClient>();
        services.AddScoped<ICatalogProductAvailabilityClient, CatalogProductAvailabilityGrpcClient>();
        services.AddScoped<IInventoryOrderReservationClient, InventoryOrderReservationGrpcClient>();

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

        services.AddGrpcClient<InventoryOrderReservation.InventoryOrderReservationClient>(options =>
        {
            options.Address = new Uri(configuration["InventoryService:BaseUrl"] ?? "https://localhost:5111");
        });

        return services;
    }

    private static string GetConnectionStringOrFallback(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            using var loggerFactory = LoggerFactory.Create(_ => { });
            loggerFactory.CreateLogger("Order.Infrastructure.Configuration.DependencyInjection")
                .LogError("Connection string '{ConnectionStringName}' was not configured. Using a fallback connection string.", connectionStringName);
            return $"Server=localhost;Port=3306;Database={connectionStringName.ToLowerInvariant()}_fallback;Uid=root;Pwd=root;";
        }

        return connectionString;
    }
}
