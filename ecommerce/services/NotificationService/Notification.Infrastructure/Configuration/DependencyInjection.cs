using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Clients;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NotificationDb")
            ?? throw new InvalidOperationException("Connection string 'NotificationDb' was not configured.");

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

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
}
