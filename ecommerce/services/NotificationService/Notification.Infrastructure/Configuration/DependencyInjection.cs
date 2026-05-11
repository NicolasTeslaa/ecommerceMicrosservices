using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Clients;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetConnectionStringOrFallback(configuration, "NotificationDb");

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddHttpClient<ICustomerContactClient, CustomerContactClient>(client =>
        {
            var baseUrl = configuration["CustomerService:BaseUrl"] ?? "http://localhost:5117";
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHostedService<NotificationConsumerService>();
        services.AddHostedService<NotificationDispatchService>();

        return services;
    }

    private static string GetConnectionStringOrFallback(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            using var loggerFactory = LoggerFactory.Create(_ => { });
            loggerFactory.CreateLogger("Notification.Infrastructure.Configuration.DependencyInjection")
                .LogError("Connection string '{ConnectionStringName}' was not configured. Using a fallback connection string.", connectionStringName);
            return $"Server=localhost;Port=3306;Database={connectionStringName.ToLowerInvariant()}_fallback;Uid=root;Pwd=root;";
        }

        return connectionString;
    }
}
