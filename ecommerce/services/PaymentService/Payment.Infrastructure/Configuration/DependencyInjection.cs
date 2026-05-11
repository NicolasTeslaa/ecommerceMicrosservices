using ECommerce.Shared.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Payment.Application.Interfaces;
using Payment.Infrastructure.Clients;
using Payment.Infrastructure.Messaging;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Webhooks;

namespace Payment.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetConnectionStringOrFallback(configuration, "PaymentDb");

        services.Configure<StripeOptions>(configuration.GetSection("Stripe"));

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36))));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IOrderPaymentAccessClient, OrderPaymentAccessGrpcClient>();
        services.AddScoped<IStripePaymentGateway, StripePaymentGateway>();
        services.AddScoped<IPaymentEventPublisher, KafkaPaymentEventPublisher>();
        services.AddScoped<IStripeWebhookHandler, StripeWebhookHandler>();
        services.AddHostedService<OrderPendingPaymentConsumerService>();
        services.AddHostedService<PaymentOutboxDispatcherService>();

        services.AddGrpcClient<OrderPaymentAccess.OrderPaymentAccessClient>(options =>
        {
            options.Address = new Uri(configuration["OrderService:ReadBaseUrl"] ?? "https://localhost:5103");
        });

        return services;
    }

    private static string GetConnectionStringOrFallback(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            using var loggerFactory = LoggerFactory.Create(_ => { });
            loggerFactory.CreateLogger("Payment.Infrastructure.Configuration.DependencyInjection")
                .LogError("Connection string '{ConnectionStringName}' was not configured. Using a fallback connection string.", connectionStringName);
            return $"Server=localhost;Port=3306;Database={connectionStringName.ToLowerInvariant()}_fallback;Uid=root;Pwd=root;";
        }

        return connectionString;
    }
}
