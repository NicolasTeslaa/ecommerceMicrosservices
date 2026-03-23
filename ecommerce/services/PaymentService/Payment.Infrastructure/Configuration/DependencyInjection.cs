using ECommerce.Shared.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        var connectionString = GetRequiredConnectionString(configuration, "PaymentDb");

        services.Configure<StripeOptions>(configuration.GetSection("Stripe"));

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)));

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

    private static string GetRequiredConnectionString(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' was not configured.");

        return connectionString;
    }
}
